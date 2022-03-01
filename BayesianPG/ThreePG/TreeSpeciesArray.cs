using BayesianPG.Extensions;
using System;
using System.Collections.Generic;

namespace BayesianPG.ThreePG
{
    public class TreeSpeciesArray
    {
        public int n_sp { get; private set; }
        public string[] Species { get; private set; }

        public TreeSpeciesArray()
        {
            this.n_sp = 0;
            this.Species = Array.Empty<string>();
        }

        public virtual void AllocateSpecies(int additionalSpecies)
        {
            if (additionalSpecies < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(additionalSpecies));
            }

            this.Species = this.Species.Resize(this.n_sp + additionalSpecies);
            this.n_sp += additionalSpecies;
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
            for (int index = 0; index < this.Species.Length; ++index)
            {
                if (uniqueNames.Add(this.Species[index]) == false)
                {
                    throw new ArgumentException("Species " + this.Species[index] + " is already present.", nameof(names));
                }
            }

            // append names
            int existingSpecies = this.n_sp;
            this.AllocateSpecies(names.Length);
            Array.Copy(names, 0, this.Species, existingSpecies, names.Length);
        }

        public bool SpeciesMatch(TreeSpeciesArray other)
        {
            if (this.n_sp != other.n_sp)
            {
                return false;
            }

            for (int index = 0; index < this.n_sp; ++index)
            {
                if (String.Equals(this.Species[index], other.Species[index], StringComparison.Ordinal) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}