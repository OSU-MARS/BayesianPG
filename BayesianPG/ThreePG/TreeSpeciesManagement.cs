using BayesianPG.Extensions;
using System;

namespace BayesianPG.ThreePG
{
    /// <summary>
    /// management: produced from d_thinning by prepare_thinning() in R, managementInputs in Fortran
    /// [n_sp][n_man]
    /// </summary>
    public class TreeSpeciesManagement : TreeSpeciesArray
    {
        public static TreeSpeciesManagement None { get; private set; }

        public int[] n_man { get; private set; }

        /// <summary>
        /// age when thinning is performed
        /// </summary>
        public float[][] age { get; private set; }

        /// <summary>
        /// number of trees remaining after thinning
        /// </summary>
        public float[][] stems_n { get; private set; }

        /// <summary>
        /// type of thinning(above/below). Default is 1
        /// </summary>
        public float[][] stem { get; private set; }

        /// <summary>
        /// type of thinning(above/below). Default is 1
        /// </summary>
        public float[][] root { get; private set; }

        /// <summary>
        /// type of thinning(above/below). Default is 1
        /// </summary>
        public float[][] foliage { get; private set; }

        static TreeSpeciesManagement()
        {
            TreeSpeciesManagement.None = new();
        }

        public TreeSpeciesManagement()
        {
            this.n_man = Array.Empty<int>();

            this.age = Array.Empty<float[]>();
            this.stems_n = Array.Empty<float[]>();
            this.stem = Array.Empty<float[]>();
            this.root = Array.Empty<float[]>();
            this.foliage = Array.Empty<float[]>();
        }

        public int AllocateManagement(int speciesIndex)
        {
            int n_man = ++this.n_man[speciesIndex];
            this.n_man[speciesIndex] = n_man;

            if (n_man == 1)
            {
                this.age[speciesIndex] = new float[1];
                this.stems_n[speciesIndex] = new float[1];
                this.stem[speciesIndex] = new float[1];
                this.root[speciesIndex] = new float[1];
                this.foliage[speciesIndex] = new float[1];
            }
            else
            {
                this.age[speciesIndex] = this.age[speciesIndex].Resize(n_man);
                this.stems_n[speciesIndex] = this.stems_n[speciesIndex].Resize(n_man);
                this.stem[speciesIndex] = this.stem[speciesIndex].Resize(n_man);
                this.root[speciesIndex] = this.root[speciesIndex].Resize(n_man);
                this.foliage[speciesIndex] = this.foliage[speciesIndex].Resize(n_man);
            }

            return n_man - 1; // index of newly added management
        }

        public override void AllocateSpecies(string[] names)
        {
            base.AllocateSpecies(names);

            this.age = this.age.Resize(this.n_sp);
            this.n_man = this.n_man.Resize(this.n_sp);
            this.stems_n = this.stems_n.Resize(this.n_sp);
            this.stem = this.stem.Resize(this.n_sp);
            this.root = this.root.Resize(this.n_sp);
            this.foliage = this.foliage.Resize(this.n_sp);
        }
    }
}
