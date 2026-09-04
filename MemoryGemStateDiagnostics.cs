using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum MemoryGemStatePhase
    {
        Observation,
        Before,
        After
    }

    internal sealed class MemoryGemStateObservation
    {
        internal int FlowId;
        internal string Trigger;
        internal MemoryGemStatePhase Phase;
        internal string Role;
        internal string SubjectKind;
        internal string Identity;
        internal string PlayerIdentity;
        internal int? MemoryLevel;
        internal float? MemoryQuality { get; set; }
        internal float? GemQuality;
        internal int? GemEffectiveLevel;
        internal bool? IsEquipped;
        internal bool IsCandidate;
        internal string Slot;
        internal string ParentMemory;
        internal string NativeElement;
        internal string EffectiveElement;
        internal readonly List<KeyValuePair<string, string>> NativeValues = new List<KeyValuePair<string, string>>();
        internal readonly List<KeyValuePair<string, string>> ContextualValues = new List<KeyValuePair<string, string>>();
        internal readonly List<KeyValuePair<string, string>> BuildContext = new List<KeyValuePair<string, string>>();
        internal readonly List<string> AttachedGems = new List<string>();
    }

    internal static class MemoryGemStateObservationFormatter
    {
        internal const string Marker = "memory-gem-state";

        internal static string Format(MemoryGemStateObservation observation)
        {
            var builder = new StringBuilder(1024);
            builder.Append(Marker);
            Append(builder, "phase", observation != null ? observation.Phase.ToString().ToUpperInvariant() : "UNKNOWN");
            Append(builder, "flow", observation != null && observation.FlowId > 0 ? observation.FlowId.ToString(CultureInfo.InvariantCulture) : "none");
            Append(builder, "trigger", observation != null ? observation.Trigger : null);
            Append(builder, "role", observation != null ? observation.Role : null);
            Append(builder, "subjectKind", observation != null ? observation.SubjectKind : null);
            Append(builder, "identity", observation != null ? observation.Identity : null);
            Append(builder, "player", observation != null ? observation.PlayerIdentity : null);
            Append(builder, "memoryLevel", FormatNullable(observation != null ? observation.MemoryLevel : null));
            Append(builder, "memoryQuality", FormatNullable(observation != null ? observation.MemoryQuality : null));
            Append(builder, "gemQuality", FormatNullable(observation != null ? observation.GemQuality : null));
            Append(builder, "gemEffectiveLevel", FormatNullable(observation != null ? observation.GemEffectiveLevel : null));
            Append(builder, "equipped", FormatNullable(observation != null ? observation.IsEquipped : null));
            Append(builder, "candidate", observation != null && observation.IsCandidate ? "true" : "false");
            Append(builder, "slot", observation != null ? observation.Slot : null);
            Append(builder, "parentMemory", observation != null ? observation.ParentMemory : null);
            Append(builder, "nativeElement", observation != null ? observation.NativeElement : null);
            Append(builder, "effectiveElement", observation != null ? observation.EffectiveElement : null);
            AppendList(builder, "attachedGems", observation != null ? observation.AttachedGems : null);
            AppendPairs(builder, "native", observation != null ? observation.NativeValues : null);
            AppendPairs(builder, "contextual", observation != null ? observation.ContextualValues : null);
            AppendPairs(builder, "build", observation != null ? observation.BuildContext : null);
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string key, string value)
        {
            builder.Append(' ').Append(key).Append('=').Append(Escape(value));
        }

        private static void AppendList(StringBuilder builder, string key, List<string> values)
        {
            builder.Append(' ').Append(key).Append("=[");
            if (values == null || values.Count == 0)
            {
                builder.Append("none");
            }
            else
            {
                var copy = new List<string>(values);
                copy.Sort(StringComparer.Ordinal);
                for (var i = 0; i < copy.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    builder.Append(Escape(copy[i]));
                }
            }

            builder.Append(']');
        }

        private static void AppendPairs(StringBuilder builder, string key, List<KeyValuePair<string, string>> values)
        {
            builder.Append(' ').Append(key).Append("=[");
            if (values == null || values.Count == 0)
            {
                builder.Append("none");
            }
            else
            {
                var copy = new List<KeyValuePair<string, string>>(values);
                copy.Sort((left, right) => StringComparer.Ordinal.Compare(left.Key, right.Key));
                for (var i = 0; i < copy.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    builder.Append(Escape(copy[i].Key)).Append(':').Append(Escape(copy[i].Value));
                }
            }

            builder.Append(']');
        }

        private static string Escape(string value)
        {
            return Uri.EscapeDataString(string.IsNullOrEmpty(value) ? "unknown" : value);
        }

        private static string FormatNullable(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "unknown";
        }

        private static string FormatNullable(float? value)
        {
            return value.HasValue && !float.IsNaN(value.Value) && !float.IsInfinity(value.Value)
                ? value.Value.ToString("0.###", CultureInfo.InvariantCulture)
                : "unknown";
        }

        private static string FormatNullable(bool? value)
        {
            return value.HasValue ? (value.Value ? "true" : "false") : "unknown";
        }
    }

    internal static class MemoryGemStateDiagnostics
    {
        private const int NativeFieldLimit = 32;
        private static readonly HashSet<string> EmittedTransitionKeys = new HashSet<string>(StringComparer.Ordinal);

        internal static void Reset()
        {
            EmittedTransitionKeys.Clear();
        }

        internal static string Capture(
            int flowId,
            string trigger,
            MemoryGemStatePhase phase,
            string role,
            Actor subject,
            HeroSkill heroSkill,
            bool isCandidate)
        {
            var key = BuildTransitionKey(flowId, trigger, phase, role, subject);
            if (!ShouldEmit(key))
            {
                return null;
            }

            return MemoryGemStateObservationFormatter.Format(BuildObservation(flowId, trigger, phase, role, subject, heroSkill, isCandidate));
        }

        internal static string BuildTransitionKey(int flowId, string trigger, MemoryGemStatePhase phase, string role, Actor subject)
        {
            return flowId.ToString(CultureInfo.InvariantCulture) + "|" + (trigger ?? "") + "|" + phase + "|" + (role ?? "") + "|" +
                (subject != null ? subject.GetInstanceID().ToString(CultureInfo.InvariantCulture) : "null");
        }

        internal static bool ShouldEmit(string transitionKey)
        {
            return EmittedTransitionKeys.Add(transitionKey ?? "unknown");
        }

        private static MemoryGemStateObservation BuildObservation(
            int flowId,
            string trigger,
            MemoryGemStatePhase phase,
            string role,
            Actor subject,
            HeroSkill heroSkill,
            bool isCandidate)
        {
            if (heroSkill == null)
            {
                heroSkill = ResolveHeroSkill(subject);
            }

            var hero = heroSkill != null ? heroSkill.hero : ResolveOwnerHero(subject);
            var observation = new MemoryGemStateObservation
            {
                FlowId = flowId,
                Trigger = trigger,
                Phase = phase,
                Role = role,
                SubjectKind = subject is SkillTrigger ? "Memory" : subject is Gem ? "Gem" : "Unknown",
                Identity = subject != null ? subject.GetType().Name : null,
                PlayerIdentity = FormatPlayer(hero != null ? hero.owner : null),
                MemoryLevel = subject is SkillTrigger skill ? (int?)skill.level : null,
                GemQuality = subject is Gem gem ? (float?)gem.quality : null,
                GemEffectiveLevel = subject is Gem effectiveGem ? (int?)effectiveGem.effectiveLevel : null,
                IsCandidate = isCandidate,
                Slot = "unknown",
                ParentMemory = "unknown",
                NativeElement = "unknown",
                EffectiveElement = "unknown"
            };

            CaptureAssociation(observation, subject, heroSkill);
            CaptureNativeValues(observation.NativeValues, observation.ContextualValues, subject, hero);
            observation.NativeElement = ResolveNativeElement(observation.NativeValues);
            CaptureBuildContext(observation.BuildContext, hero);
            CaptureAttachedGems(observation.AttachedGems, subject as SkillTrigger, heroSkill);
            return observation;
        }

        private static void CaptureAssociation(MemoryGemStateObservation observation, Actor subject, HeroSkill heroSkill)
        {
            if (subject == null || heroSkill == null)
            {
                observation.IsEquipped = null;
                return;
            }

            if (subject is SkillTrigger skill)
            {
                if (heroSkill.TryGetSkillLocation(skill, out var location))
                {
                    observation.IsEquipped = true;
                    observation.Slot = location.ToString();
                }
                else
                {
                    observation.IsEquipped = false;
                }

                return;
            }

            if (subject is Gem gem)
            {
                if (heroSkill.TryGetGemLocation(gem, out var location))
                {
                    observation.IsEquipped = true;
                    observation.Slot = location.skill + ":" + location.index.ToString(CultureInfo.InvariantCulture);
                    var memory = heroSkill.GetSkill(location.skill);
                    observation.ParentMemory = memory != null ? memory.GetType().Name : "unknown";
                }
                else
                {
                    observation.IsEquipped = false;
                }
            }
        }

        private static void CaptureAttachedGems(List<string> target, SkillTrigger memory, HeroSkill heroSkill)
        {
            if (memory == null || heroSkill == null || !heroSkill.TryGetSkillLocation(memory, out var location))
            {
                return;
            }

            foreach (var pair in heroSkill.GetGemsPairInSkill(location))
            {
                if (pair.Value != null)
                {
                    target.Add(pair.Key.index.ToString(CultureInfo.InvariantCulture) + ":" + pair.Value.GetType().Name + ":quality=" +
                        pair.Value.quality.ToString(CultureInfo.InvariantCulture) + ":effectiveLevel=" + pair.Value.effectiveLevel.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private static void CaptureBuildContext(List<KeyValuePair<string, string>> target, Hero hero)
        {
            if (hero == null || hero.Status == null)
            {
                Add(target, "finalAD", null);
                Add(target, "finalAP", null);
                Add(target, "abilityHaste", null);
                return;
            }

            Add(target, "finalAD", FormatFloat(hero.Status.attackDamage));
            Add(target, "finalAP", FormatFloat(hero.Status.abilityPower));
            Add(target, "abilityHaste", FormatFloat(hero.Status.abilityHaste));
            Add(target, "armor", FormatFloat(hero.Status.armor));
            Add(target, "maxHealth", FormatFloat(hero.Status.maxHealth));
        }

        private static string ResolveNativeElement(List<KeyValuePair<string, string>> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (values[i].Key.IndexOf("element", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    !string.Equals(values[i].Value, "unknown", StringComparison.Ordinal))
                {
                    return values[i].Value;
                }
            }

            return "unknown";
        }

        private static void CaptureNativeValues(
            List<KeyValuePair<string, string>> target,
            List<KeyValuePair<string, string>> contextualTarget,
            Actor subject,
            Hero hero)
        {
            if (subject == null)
            {
                Add(target, "readability", null);
                return;
            }

            var level = subject is SkillTrigger skill ? skill.level : subject is Gem gem ? gem.effectiveLevel : 1;
            var fields = new List<FieldInfo>();
            var type = subject.GetType();
            while (type != null && type != typeof(Actor))
            {
                fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }

            fields.Sort((left, right) => StringComparer.Ordinal.Compare(left.DeclaringType.Name + "." + left.Name, right.DeclaringType.Name + "." + right.Name));
            for (var i = 0; i < fields.Count && target.Count < NativeFieldLimit; i++)
            {
                var field = fields[i];
                if (!IsReadableNativeField(field))
                {
                    continue;
                }

                var key = field.DeclaringType.Name + "." + field.Name;
                try
                {
                    AddNativeField(target, contextualTarget, key, field.GetValue(subject), level, hero);
                }
                catch
                {
                    Add(target, key, null);
                }
            }

            if (subject is AbilityTrigger trigger)
            {
                CaptureConfigs(target, trigger);
            }
        }

        private static bool IsReadableNativeField(FieldInfo field)
        {
            if (field.FieldType == typeof(ScalingValue) || field.FieldType == typeof(CastMethodData))
            {
                return true;
            }

            var name = field.Name.ToLowerInvariant();
            if (field.FieldType.IsEnum && name.Contains("element"))
            {
                return true;
            }

            return (field.FieldType == typeof(float) || field.FieldType == typeof(int)) &&
                (name.Contains("damage") || name.Contains("dmg") || name.Contains("cooldown") || name.Contains("charge") ||
                 name.Contains("range") || name.Contains("radius") || name.Contains("area") || name.Contains("count") || name.Contains("hit"));
        }

        private static void AddNativeField(
            List<KeyValuePair<string, string>> target,
            List<KeyValuePair<string, string>> contextualTarget,
            string key,
            object value,
            int level,
            Hero hero)
        {
            if (value == null)
            {
                Add(target, key, null);
            }
            else if (value is ScalingValue scaling)
            {
                Add(target, key + ".base", FormatFloat(scaling.baseValue));
                Add(target, key + ".adCoefficient", FormatFloat(scaling.adFactor));
                Add(target, key + ".apCoefficient", FormatFloat(scaling.apFactor));
                Add(target, key + ".levelCoefficient", FormatFloat(scaling.lvlFactor));
                Add(target, key + ".leveling", scaling.leveling.ToString());
                Add(target, key + ".scalingMultiplier", FormatFloat(scaling.scalingMultiplier));
                Add(contextualTarget, key + ".observedValue", TryEvaluate(scaling, level, hero));
            }
            else if (value is CastMethodData cast)
            {
                Add(target, key + ".range", TryRead(() => cast.pointData.range));
                Add(target, key + ".radius", TryRead(() => cast.pointData.radius));
                Add(target, key + ".areaAngle", TryRead(() => cast.coneData.angle));
                Add(target, key + ".areaWidth", TryRead(() => cast.arrowData.width));
                Add(target, key + ".areaLength", TryRead(() => cast.arrowData.length));
            }
            else if (value is float number)
            {
                Add(target, key, FormatFloat(number));
            }
            else
            {
                Add(target, key, Convert.ToString(value, CultureInfo.InvariantCulture));
            }
        }

        private static void CaptureConfigs(List<KeyValuePair<string, string>> target, AbilityTrigger trigger)
        {
            if (trigger.configs == null)
            {
                Add(target, "configs", null);
                return;
            }

            for (var i = 0; i < trigger.configs.Length && target.Count < NativeFieldLimit; i++)
            {
                var config = trigger.configs[i];
                var prefix = "config." + i.ToString(CultureInfo.InvariantCulture) + ".";
                if (config == null)
                {
                    Add(target, prefix + "readability", null);
                    continue;
                }

                Add(target, prefix + "cooldown", FormatFloat(config.cooldownTime));
                Add(target, prefix + "charges", (config.maxCharges + config.addedCharges).ToString(CultureInfo.InvariantCulture));
                Add(target, prefix + "range", FormatFloat(config.effectiveRange));
            }
        }

        private static string TryEvaluate(ScalingValue value, int level, Hero hero)
        {
            try
            {
                return FormatFloat(value.GetValue(level, hero));
            }
            catch
            {
                return null;
            }
        }

        private static string TryRead(Func<float> read)
        {
            try
            {
                return FormatFloat(read());
            }
            catch
            {
                return null;
            }
        }

        private static HeroSkill ResolveHeroSkill(Actor subject)
        {
            var owner = ResolveOwnerHero(subject);
            if (owner != null)
            {
                return owner.Skill;
            }

            return DewPlayer.local != null && DewPlayer.local.hero != null ? DewPlayer.local.hero.Skill : null;
        }

        private static Hero ResolveOwnerHero(Actor subject)
        {
            if (subject is SkillTrigger skill)
            {
                return skill.owner;
            }

            if (subject is Gem gem)
            {
                return gem.owner;
            }

            return null;
        }

        private static string FormatPlayer(DewPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            return !string.IsNullOrEmpty(player.guid) ? player.guid : !string.IsNullOrEmpty(player.playerNameRaw) ? player.playerNameRaw : player.GetInstanceID().ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatFloat(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? null : value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void Add(List<KeyValuePair<string, string>> target, string key, string value)
        {
            if (target.Count < NativeFieldLimit || key == "finalAD" || key == "finalAP" || key == "abilityHaste" || key == "armor" || key == "maxHealth")
            {
                target.Add(new KeyValuePair<string, string>(key, value ?? "unknown"));
            }
        }
    }


#if DEBUG
    internal static class MemoryGemStateDiagnosticsValidation
    {
        internal static void RunAll()
        {
            ValidatesStableFormattingAndUnknowns();
            ValidatesAssociationAndFlowCorrelation();
            ValidatesDuplicateTransitionSuppression();
        }

        private static void ValidatesStableFormattingAndUnknowns()
        {
            var observation = new MemoryGemStateObservation
            {
                FlowId = 7,
                Trigger = "TEST CHANGE",
                Phase = MemoryGemStatePhase.Before,
                Role = "candidate",
                SubjectKind = "Gem",
                Identity = "Gem_Test",
                IsCandidate = true
            };
            observation.NativeValues.Add(new KeyValuePair<string, string>("z", "2"));
            observation.NativeValues.Add(new KeyValuePair<string, string>("a", "1"));

            var formatted = MemoryGemStateObservationFormatter.Format(observation);
            AssertContains(formatted, "flow=7", "flow formatting");
            AssertContains(formatted, "trigger=TEST%20CHANGE", "escaped trigger");
            AssertContains(formatted, "memoryLevel=unknown", "unknown memory level");
            AssertContains(formatted, "native=[a:1;z:2]", "stable field ordering");
        }

        private static void ValidatesAssociationAndFlowCorrelation()
        {
            var before = new MemoryGemStateObservation
            {
                FlowId = 12,
                Trigger = "UPGRADE",
                Phase = MemoryGemStatePhase.Before,
                Role = "target",
                SubjectKind = "Memory",
                Identity = "Mem_Test",
                MemoryLevel = 2,
                IsEquipped = true,
                Slot = "Q"
            };
            before.AttachedGems.Add("0:Gem_Test:quality=3");
            var after = new MemoryGemStateObservation
            {
                FlowId = 12,
                Trigger = "UPGRADE",
                Phase = MemoryGemStatePhase.After,
                Role = "target",
                SubjectKind = "Memory",
                Identity = "Mem_Test",
                MemoryLevel = 3,
                IsEquipped = true,
                Slot = "Q"
            };

            AssertContains(MemoryGemStateObservationFormatter.Format(before), "attachedGems=[0%3AGem_Test%3Aquality%3D3]", "Memory Gem association");
            AssertContains(MemoryGemStateObservationFormatter.Format(after), "flow=12", "after flow correlation");
        }

        private static void ValidatesDuplicateTransitionSuppression()
        {
            MemoryGemStateDiagnostics.Reset();
            if (!MemoryGemStateDiagnostics.ShouldEmit("same") || MemoryGemStateDiagnostics.ShouldEmit("same"))
            {
                throw new InvalidOperationException("memory-gem-state validation failed: duplicate transition suppression");
            }
            MemoryGemStateDiagnostics.Reset();
        }

        private static void AssertContains(string actual, string expected, string label)
        {
            if (actual == null || actual.IndexOf(expected, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException("memory-gem-state validation failed: " + label);
            }
        }
    }
#endif
}
