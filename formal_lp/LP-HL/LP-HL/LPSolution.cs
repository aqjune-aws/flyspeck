using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LP_HL
{
    /// <summary>
    /// A solution of an LP
    /// </summary>
    class LPSolution
    {
        /// <summary>
        /// Number of constraints
        /// </summary>
        public int NumberOfConstraints { get; private set; }

        /// <summary>
        /// Number of variables
        /// </summary>
        public int NumberOfVariables { get; private set; }

        /// <summary>
        /// The value of an objective function
        /// </summary>
        public LpNumber Optimal { get; private set; }

        /// <summary>
        /// The desired upper bound
        /// </summary>
        public LpNumber UpperBound { get; private set; }


        /// <summary>
        /// Marginals for constraints
        /// </summary>
        public List<LpNumber> ConstraintMarginals { get; private set; }

        /// <summary>
        /// Marginals for variables
        /// </summary>
        public List<LpNumber> VariableMarginals {get; private set; }


        /// <summary>
        /// Private constructor
        /// </summary>
        private LPSolution()
        {
            ConstraintMarginals = new List<LpNumber>();
            VariableMarginals = new List<LpNumber>();
        }


        /// <summary>
        /// Reads the dual value out of one row ("i") or column ("j") record of a
        /// glpsol solution file.  A record is
        ///     i &lt;number&gt; &lt;status&gt; &lt;primal&gt; &lt;dual&gt;
        /// except that a basic variable carries no dual, so a record of four
        /// fields has a dual of zero.
        /// </summary>
        private static LpNumber ReadDual(StreamReader r, char kind, int precision)
        {
            string str = r.ReadLine();
            while (str != null && (str.Length == 0 || str[0] == 'c'))
                str = r.ReadLine();

            if (str == null)
                throw new Exception("Unexpected end of solution file");
            if (str[0] != kind)
                throw new Exception("Expected a '" + kind + "' record, got: " + str);

            string[] els = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (els.Length < 4)
                throw new Exception("Malformed record: " + str);

            double dual = (els.Length >= 5) ? double.Parse(els[4]) : 0.0;
            return new LpNumber(dual, precision);
        }


        private static List<LpNumber> ReadDuals(StreamReader r, char kind, int k, int precision)
        {
            List<LpNumber> result = new List<LpNumber>();
            for (int i = 0; i < k; i++)
                result.Add(ReadDual(r, kind, precision));
            return result;
        }


        /// <summary>
        /// Reads a solution written by "glpsol -w".  That file is
        ///
        ///     c &lt;comments&gt;
        ///     s bas &lt;rows&gt; &lt;cols&gt; &lt;prim status&gt; &lt;dual status&gt; &lt;objective&gt;
        ///     i &lt;number&gt; &lt;status&gt; &lt;primal&gt; &lt;dual&gt;      one per row
        ///     j &lt;number&gt; &lt;status&gt; &lt;primal&gt; &lt;dual&gt;      one per column
        ///     e o f
        ///
        /// For a feasible problem the first row is the objective and is not a
        /// constraint.  For an infeasible one the objective row is absent and the
        /// first "rows" columns are the slack variables that
        /// create_infeasible_solution added, so neither is counted.
        /// </summary>
        public static LPSolution LoadSolution(StreamReader r, int precision, LpNumber upperBound, bool infeasible)
        {
            LPSolution sol = new LPSolution();
            sol.UpperBound = upperBound;

            if (infeasible)
            {
                sol.UpperBound = new LpNumber(0);
            }

            string str = r.ReadLine();
            while (str != null && (str.Length == 0 || str[0] == 'c'))
                str = r.ReadLine();

            if (str == null || str[0] != 's')
                throw new Exception("A solution file must begin with an 's' record");

            string[] els = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (els.Length != 7)
                throw new Exception("Seven fields are expected on the 's' record: " + str);

            int nc = int.Parse(els[2]);
            int nv = int.Parse(els[3]);
            sol.Optimal = new LpNumber(double.Parse(els[6]), precision);

            if (infeasible)
            {
                sol.NumberOfConstraints = nc;
                // Do not count slack variables
                sol.NumberOfVariables = nv - nc;
            }
            else
            {
                // Subtract one since we don't count the objective function for a feasible solution
                sol.NumberOfConstraints = nc - 1;
                sol.NumberOfVariables = nv;
            }

            if (!infeasible)
            {
                // Skip the objective row
                ReadDual(r, 'i', precision);
            }

            // Constraints
            sol.ConstraintMarginals = ReadDuals(r, 'i', sol.NumberOfConstraints, precision);

            if (infeasible)
            {
                // Skip slack variables
                ReadDuals(r, 'j', nc, precision);
            }

            // Bounds
            sol.VariableMarginals = ReadDuals(r, 'j', sol.NumberOfVariables, precision);

            return sol;
        }
    }
}
