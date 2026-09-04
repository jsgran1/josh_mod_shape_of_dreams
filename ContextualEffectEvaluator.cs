using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum ContextualEffectConditionKind
    {
        CharacterId,
        MemoryContentId,
        MemoryElement,
        AttachedGemContentId,
        CandidateChangeKind,
        CandidateContentId,
        FinalStatPresent,
        PersistentModifierPresent
    }

    internal enum ContextualEffectConditionState
    {
        Matched,
        NotMatched,
        Unknown
    }

    internal sealed class ContextualEffectCondition
    {
        internal ContextualEffectCondition(ContextualEffectConditionKind kind, string expectedValue)
        {
            Kind = kind;
            ExpectedValue = expectedValue ?? "";
            ExpectedChangeKind = BuildChangeKind.None;
        }

        internal ContextualEffectCondition(BuildChangeKind expectedChangeKind)
        {
            Kind = ContextualEffectConditionKind.CandidateChangeKind;
            ExpectedValue = expectedChangeKind.ToString();
            ExpectedChangeKind = expectedChangeKind;
        }

        internal ContextualEffectConditionKind Kind { get; }
        internal string ExpectedValue { get; }
        internal BuildChangeKind ExpectedChangeKind { get; }

        internal static ContextualEffectCondition Character(string characterId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.CharacterId, characterId);
        }

        internal static ContextualEffectCondition MemoryContent(string contentId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.MemoryContentId, contentId);
        }

        internal static ContextualEffectCondition MemoryElement(string element)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.MemoryElement, element);
        }

        internal static ContextualEffectCondition AttachedGemContent(string contentId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.AttachedGemContentId, contentId);
        }

        internal static ContextualEffectCondition CandidateChange(BuildChangeKind changeKind)
        {
            return new ContextualEffectCondition(changeKind);
        }

        internal static ContextualEffectCondition CandidateContent(string contentId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.CandidateContentId, contentId);
        }

        internal static ContextualEffectCondition FinalStatPresent(string statId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.FinalStatPresent, statId);
        }

        internal static ContextualEffectCondition PersistentModifierPresent(string modifierId)
        {
            return new ContextualEffectCondition(ContextualEffectConditionKind.PersistentModifierPresent, modifierId);
        }
    }

    internal sealed class ContextualEffectRule
    {
        internal ContextualEffectRule(
            ComparisonStructuredValue value,
            string genericDescription,
            string effectiveDescription,
            string effectiveElement,
            IEnumerable<ContextualEffectCondition> conditions,
            IEnumerable<string> limitations)
        {
            Value = value;
            GenericDescription = string.IsNullOrEmpty(genericDescription) && value != null ? value.Label : genericDescription ?? "";
            EffectiveDescription = string.IsNullOrEmpty(effectiveDescription) && value != null ? GetStructuredDescription(value) : effectiveDescription ?? "";
            EffectiveElement = effectiveElement ?? "";
            Conditions = ComparisonContractLists.Copy(conditions);
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal ComparisonStructuredValue Value { get; }
        internal string GenericDescription { get; }
        internal string EffectiveDescription { get; }
        internal string EffectiveElement { get; }
        internal IReadOnlyList<ContextualEffectCondition> Conditions { get; }
        internal IReadOnlyList<string> Limitations { get; }

        internal static ContextualEffectRule FromStructuredValue(ComparisonStructuredValue value)
        {
            return new ContextualEffectRule(value, null, null, ExtractElement(value), null, null);
        }

        private static string GetStructuredDescription(ComparisonStructuredValue value)
        {
            if (!string.IsNullOrEmpty(value.TextValue))
            {
                return value.TextValue;
            }

            return value.Label;
        }

        private static string ExtractElement(ComparisonStructuredValue value)
        {
            if (value == null || string.IsNullOrEmpty(value.TextValue))
            {
                return "";
            }

            if (IsElementValueId(value.ValueId))
            {
                return value.TextValue;
            }

            return "";
        }

        private static bool IsElementValueId(string valueId)
        {
            return string.Equals(valueId, "element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, "memory-element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, "damage-element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, "effective-element", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class ContextualEffectEvaluationRequest
    {
        internal ContextualEffectEvaluationRequest(
            BuildStateSnapshot buildState,
            MemoryState memoryContext,
            IEnumerable<GemState> attachedGemContext,
            CandidateChange candidateChange,
            IEnumerable<ContextualEffectRule> effectRules)
        {
            BuildState = buildState;
            MemoryContext = memoryContext;
            AttachedGemContext = ComparisonContractLists.Copy(attachedGemContext);
            CandidateChange = candidateChange;
            EffectRules = ComparisonContractLists.Copy(effectRules);
        }

        internal BuildStateSnapshot BuildState { get; }
        internal MemoryState MemoryContext { get; }
        internal IReadOnlyList<GemState> AttachedGemContext { get; }
        internal CandidateChange CandidateChange { get; }
        internal IReadOnlyList<ContextualEffectRule> EffectRules { get; }
    }

    internal static class ContextualEffectEvaluator
    {
        internal static ContextualEffectEvaluation Evaluate(
            BuildStateSnapshot buildState,
            MemoryState memoryContext,
            IEnumerable<GemState> attachedGemContext,
            CandidateChange candidateChange)
        {
            return Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryContext,
                attachedGemContext,
                candidateChange,
                null));
        }

        internal static ContextualEffectEvaluation Evaluate(ContextualEffectEvaluationRequest request)
        {
            if (request == null)
            {
                return UnknownEvaluation(
                    new ComparisonSubject(ComparisonSubjectKind.Unknown, "UNKNOWN_CONTEXT", "", "Unknown context"),
                    null,
                    null,
                    null,
                    "No contextual evaluation request was provided");
            }

            var attachedGems = ResolveAttachedGems(request);
            var subject = ResolveSubject(request);
            var candidateChange = request.CandidateChange ?? CandidateChange.None(subject);
            var rules = ResolveRules(request, attachedGems);
            var effects = new List<EffectiveEffect>();
            var evaluationLimitations = new List<string>();
            var confidence = ComparisonConfidence.Verified;

            if (rules.Count == 0)
            {
                evaluationLimitations.Add("no structured effects were available for contextual evaluation");
                effects.Add(EffectiveEffect.Unknown(
                    "contextual-effects",
                    "No structured effects were available",
                    evaluationLimitations));
                confidence = ComparisonConfidence.Unknown;
            }
            else
            {
                for (var i = 0; i < rules.Count; i++)
                {
                    var effect = EvaluateRule(rules[i], request.BuildState, request.MemoryContext, attachedGems, candidateChange);
                    effects.Add(effect);
                    confidence = ComparisonSemantics.MostConservative(confidence, effect.Confidence);
                    ComparisonSemantics.AppendLimitations(evaluationLimitations, effect.Limitations);
                }
            }

            return new ContextualEffectEvaluation(
                subject,
                request.MemoryContext,
                attachedGems,
                candidateChange,
                effects,
                confidence,
                evaluationLimitations);
        }

        private static EffectiveEffect EvaluateRule(
            ContextualEffectRule rule,
            BuildStateSnapshot buildState,
            MemoryState memoryContext,
            IReadOnlyList<GemState> attachedGemContext,
            CandidateChange candidateChange)
        {
            if (rule == null || rule.Value == null)
            {
                return EffectiveEffect.Unknown(
                    "contextual-effect",
                    "Structured effect is unavailable",
                    new[] { "structured effect rule was missing" });
            }

            var limitations = ComparisonSemantics.CopyLimitations(rule.Limitations);
            var confidence = rule.Value.Confidence;

            for (var i = 0; i < rule.Conditions.Count; i++)
            {
                var conditionResult = EvaluateCondition(rule.Conditions[i], buildState, memoryContext, attachedGemContext, candidateChange);
                confidence = ComparisonSemantics.MostConservative(confidence, conditionResult.Confidence);

                if (conditionResult.State == ContextualEffectConditionState.Matched)
                {
                    continue;
                }

                limitations.Add(conditionResult.Limitation);
                if (conditionResult.State == ContextualEffectConditionState.Unknown)
                {
                    return EffectiveEffect.Unknown(rule.Value.ValueId, rule.GenericDescription, limitations);
                }

                return EffectiveEffect.Unknown(rule.Value.ValueId, rule.GenericDescription, limitations);
            }

            ComparisonSemantics.AppendLimitations(limitations, rule.Value.Limitations);

            if (rule.Value.ResultClass == ComparisonResultClass.Unsupported || rule.Value.Confidence == ComparisonConfidence.Unsupported)
            {
                return EffectiveEffect.Unsupported(rule.Value.ValueId, rule.GenericDescription, limitations);
            }

            if (rule.Value.IsUnknownOrUnsupported)
            {
                return EffectiveEffect.Unknown(rule.Value.ValueId, rule.GenericDescription, limitations);
            }

            return new EffectiveEffect(
                rule.Value.ValueId,
                rule.GenericDescription,
                rule.EffectiveDescription,
                null,
                rule.Value.NumericValue,
                null,
                null,
                rule.Value.Unit,
                rule.EffectiveElement,
                rule.Value.ResultClass,
                confidence,
                limitations);
        }

        private static ContextualEffectConditionResult EvaluateCondition(
            ContextualEffectCondition condition,
            BuildStateSnapshot buildState,
            MemoryState memoryContext,
            IReadOnlyList<GemState> attachedGemContext,
            CandidateChange candidateChange)
        {
            if (condition == null)
            {
                return ContextualEffectConditionResult.Unknown("condition was missing");
            }

            switch (condition.Kind)
            {
                case ContextualEffectConditionKind.CharacterId:
                    return MatchString(buildState != null ? buildState.CharacterId : null, condition.ExpectedValue, "character context");
                case ContextualEffectConditionKind.MemoryContentId:
                    return MatchString(memoryContext != null ? memoryContext.ContentId : null, condition.ExpectedValue, "memory context");
                case ContextualEffectConditionKind.MemoryElement:
                    return MatchMemoryElement(memoryContext, condition.ExpectedValue);
                case ContextualEffectConditionKind.AttachedGemContentId:
                    return MatchAttachedGem(attachedGemContext, condition.ExpectedValue);
                case ContextualEffectConditionKind.CandidateChangeKind:
                    if (candidateChange == null)
                    {
                        return ContextualEffectConditionResult.Unknown("candidate change context is unavailable");
                    }

                    return candidateChange.ChangeKind == condition.ExpectedChangeKind
                        ? ContextualEffectConditionResult.Matched(ComparisonConfidence.Verified)
                        : ContextualEffectConditionResult.NotMatched("candidate change did not match " + condition.ExpectedValue);
                case ContextualEffectConditionKind.CandidateContentId:
                    return MatchString(candidateChange != null ? candidateChange.Candidate.ContentId : null, condition.ExpectedValue, "candidate context");
                case ContextualEffectConditionKind.FinalStatPresent:
                    return MatchFinalStat(buildState, condition.ExpectedValue);
                case ContextualEffectConditionKind.PersistentModifierPresent:
                    return MatchPersistentModifier(buildState, condition.ExpectedValue);
                default:
                    return ContextualEffectConditionResult.Unknown("condition kind is unsupported");
            }
        }

        private static IReadOnlyList<GemState> ResolveAttachedGems(ContextualEffectEvaluationRequest request)
        {
            var supplied = new List<GemState>(request.AttachedGemContext);
            if (supplied.Count > 0 || request.BuildState == null || request.MemoryContext == null)
            {
                return supplied.AsReadOnly();
            }

            for (var i = 0; i < request.BuildState.Gems.Count; i++)
            {
                var gem = request.BuildState.Gems[i];
                if (gem.AttachedMemoryKey.HasValue && gem.AttachedMemoryKey.Value.Equals(request.MemoryContext.MemoryKey))
                {
                    supplied.Add(gem);
                    continue;
                }

                for (var j = 0; j < request.MemoryContext.AttachedGemKeys.Count; j++)
                {
                    if (gem.GemKey.Equals(request.MemoryContext.AttachedGemKeys[j]))
                    {
                        supplied.Add(gem);
                        break;
                    }
                }
            }

            return supplied.AsReadOnly();
        }

        private static ComparisonSubject ResolveSubject(ContextualEffectEvaluationRequest request)
        {
            if (request.CandidateChange != null && request.CandidateChange.HasChange)
            {
                return request.CandidateChange.Candidate;
            }

            if (request.MemoryContext != null)
            {
                return new ComparisonSubject(
                    ComparisonSubjectKind.Memory,
                    request.MemoryContext.MemoryKey.StableId,
                    request.MemoryContext.ContentId,
                    request.MemoryContext.MemoryKey.DisplayName);
            }

            return new ComparisonSubject(ComparisonSubjectKind.Unknown, "UNKNOWN_CONTEXT", "", "Unknown context");
        }

        private static IReadOnlyList<ContextualEffectRule> ResolveRules(ContextualEffectEvaluationRequest request, IReadOnlyList<GemState> attachedGems)
        {
            var rules = new List<ContextualEffectRule>();
            if (request.EffectRules.Count > 0)
            {
                rules.AddRange(request.EffectRules);
                return rules.AsReadOnly();
            }

            if (request.MemoryContext != null)
            {
                AddStructuredRules(rules, request.MemoryContext.StructuredValues);
            }

            for (var i = 0; i < attachedGems.Count; i++)
            {
                AddStructuredRules(rules, attachedGems[i].StructuredValues);
            }

            return rules.AsReadOnly();
        }

        private static void AddStructuredRules(List<ContextualEffectRule> rules, IEnumerable<ComparisonStructuredValue> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                rules.Add(ContextualEffectRule.FromStructuredValue(value));
            }
        }

        private static ContextualEffectConditionResult MatchMemoryElement(MemoryState memoryContext, string expectedElement)
        {
            if (memoryContext == null)
            {
                return ContextualEffectConditionResult.Unknown("memory context is unavailable");
            }

            for (var i = 0; i < memoryContext.StructuredValues.Count; i++)
            {
                var value = memoryContext.StructuredValues[i];
                if (!IsElementValue(value))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(value.TextValue))
                {
                    return ContextualEffectConditionResult.Unknown("memory element is unavailable");
                }

                return string.Equals(value.TextValue, expectedElement, StringComparison.OrdinalIgnoreCase)
                    ? ContextualEffectConditionResult.Matched(value.Confidence)
                    : ContextualEffectConditionResult.NotMatched("memory element did not match " + expectedElement);
            }

            return ContextualEffectConditionResult.Unknown("memory element is unavailable");
        }

        private static bool IsElementValue(ComparisonStructuredValue value)
        {
            if (value == null)
            {
                return false;
            }

            return string.Equals(value.ValueId, "element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.ValueId, "memory-element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.ValueId, "damage-element", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.ValueId, "effective-element", StringComparison.OrdinalIgnoreCase);
        }

        private static ContextualEffectConditionResult MatchAttachedGem(IReadOnlyList<GemState> attachedGemContext, string expectedContentId)
        {
            if (attachedGemContext == null)
            {
                return ContextualEffectConditionResult.Unknown("attached Gem context is unavailable");
            }

            for (var i = 0; i < attachedGemContext.Count; i++)
            {
                if (string.Equals(attachedGemContext[i].ContentId, expectedContentId, StringComparison.Ordinal))
                {
                    return ContextualEffectConditionResult.Matched(ComparisonConfidence.Verified);
                }
            }

            return ContextualEffectConditionResult.NotMatched("attached Gem context did not include " + expectedContentId);
        }

        private static ContextualEffectConditionResult MatchFinalStat(BuildStateSnapshot buildState, string statId)
        {
            if (buildState == null)
            {
                return ContextualEffectConditionResult.Unknown("build-state context is unavailable");
            }

            for (var i = 0; i < buildState.FinalStats.Count; i++)
            {
                if (string.Equals(buildState.FinalStats[i].StatId, statId, StringComparison.Ordinal))
                {
                    return ContextualEffectConditionResult.Matched(buildState.FinalStats[i].Confidence);
                }
            }

            return ContextualEffectConditionResult.NotMatched("final stat context did not include " + statId);
        }

        private static ContextualEffectConditionResult MatchPersistentModifier(BuildStateSnapshot buildState, string modifierId)
        {
            if (buildState == null)
            {
                return ContextualEffectConditionResult.Unknown("build-state context is unavailable");
            }

            for (var i = 0; i < buildState.PersistentModifiers.Count; i++)
            {
                if (string.Equals(buildState.PersistentModifiers[i].ModifierId, modifierId, StringComparison.Ordinal))
                {
                    return ContextualEffectConditionResult.Matched(ComparisonConfidence.Verified);
                }
            }

            return ContextualEffectConditionResult.NotMatched("persistent modifier context did not include " + modifierId);
        }

        private static ContextualEffectConditionResult MatchString(string actual, string expected, string contextLabel)
        {
            if (string.IsNullOrEmpty(actual))
            {
                return ContextualEffectConditionResult.Unknown(contextLabel + " is unavailable");
            }

            return string.Equals(actual, expected, StringComparison.Ordinal)
                ? ContextualEffectConditionResult.Matched(ComparisonConfidence.Verified)
                : ContextualEffectConditionResult.NotMatched(contextLabel + " did not match " + expected);
        }

        private static ContextualEffectEvaluation UnknownEvaluation(
            ComparisonSubject subject,
            MemoryState memoryContext,
            IEnumerable<GemState> attachedGemContext,
            CandidateChange candidateChange,
            string limitation)
        {
            return new ContextualEffectEvaluation(
                subject,
                memoryContext,
                attachedGemContext,
                candidateChange ?? CandidateChange.None(subject),
                new[] { EffectiveEffect.Unknown("contextual-effects", "Contextual effect evaluation unavailable", new[] { limitation }) },
                ComparisonConfidence.Unknown,
                new[] { limitation });
        }
    }

    internal sealed class ContextualEffectConditionResult
    {
        private ContextualEffectConditionResult(
            ContextualEffectConditionState state,
            ComparisonConfidence confidence,
            string limitation)
        {
            State = state;
            Confidence = confidence;
            Limitation = limitation ?? "";
        }

        internal ContextualEffectConditionState State { get; }
        internal ComparisonConfidence Confidence { get; }
        internal string Limitation { get; }

        internal static ContextualEffectConditionResult Matched(ComparisonConfidence confidence)
        {
            return new ContextualEffectConditionResult(ContextualEffectConditionState.Matched, confidence, "");
        }

        internal static ContextualEffectConditionResult NotMatched(string limitation)
        {
            return new ContextualEffectConditionResult(ContextualEffectConditionState.NotMatched, ComparisonConfidence.Partial, limitation);
        }

        internal static ContextualEffectConditionResult Unknown(string limitation)
        {
            return new ContextualEffectConditionResult(ContextualEffectConditionState.Unknown, ComparisonConfidence.Unknown, limitation);
        }
    }
}
