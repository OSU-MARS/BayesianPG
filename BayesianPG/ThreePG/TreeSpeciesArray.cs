using System;
using System.Collections.Generic;

namespace BayesianPG.ThreePG
{
    public class TreeSpeciesArray
    {
        public int n_sp { get; private set; }
        public string[] Name { get; private set; }

        public TreeSpeciesArray()
        {
            this.n_sp = 0;
            this.Name = Array.Empty<string>();
        }

        public virtual void AllocateSpecies(string[] names)
        {
            // nothing to do
            if (names.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(names));
            }

            // verify species names are unique
            HashSet<string> uniqueNames = new(names);
            for (int index = 0; index < this.Name.Length; ++index)
            {
                if (uniqueNames.Add(this.Name[index]) == false)
                {
                    throw new ArgumentException("Species " + this.Name[index] + " is already present.", nameof(names));
                }
            }

            // append names
            this.Name = this.Name.Resize(this.n_sp + names.Length);
            Array.Copy(names, 0, this.Name, this.n_sp, names.Length);
            this.n_sp += names.Length;
        }

        public bool SpeciesMatch(TreeSpeciesArray other)
        {
            if (this.n_sp != other.n_sp)
            {
                return false;
            }

            for (int index = 0; index < this.n_sp; ++index)
            {
                if (String.Equals(this.Name[index], other.Name[index], StringComparison.Ordinal) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}