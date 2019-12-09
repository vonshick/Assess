using System.Collections.Generic;

namespace DataModel.Input
{
    public class Criterion
    {
        public Criterion() { }
        public Criterion(string name, string criterionDirection)
        {
            Name = name;
            CriterionDirection = criterionDirection;
        }
        public Criterion(string name, string description, string criterionDirection, int linearSegments)
        {
            Name = name;
            Description = description;
            CriterionDirection = criterionDirection;
            LinearSegments = linearSegments;
        }

        protected bool Equals(Criterion other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Criterion) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public string ID { get; set; }
        public bool IsEnum {get; set; } = false;
        public Dictionary<string, float> EnumDictionary {get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public enum CriterionDirectionTypes { Gain, Cost, Ordinal };
        public string CriterionDirection { get; set; }
        public int LinearSegments { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
    }
}
