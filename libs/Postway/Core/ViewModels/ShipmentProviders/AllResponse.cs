using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Postway.ViewModels.ShipmentProviders
{
    public class AllResponse
    {
        public string name { get; set; } = "";
        public string display_name { get; set; } = "";
        public Width width { get; set; } = new Width();
        public Length length { get; set; } = new Length();
        public Height height { get; set; } = new Height();
        public Weight weight { get; set; } = new Weight();
        public Cubic cubic { get; set; } = new Cubic();
        public Dimension dimension { get; set; } = new Dimension();
        public Cod cod { get; set; } = new Cod();
        public Insurance insurance { get; set; } = new Insurance();
        public Fee fee { get; set; } = new Fee();

        public class Weight
        {
            public bool is_calculate { get; set; }
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Width
        {
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Cod
        {
            public bool enable { get; set; }
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Cubic
        {
            public bool is_calculate { get; set; }
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Dimension
        {
            public bool is_calculate { get; set; }
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Fee
        {
            public int cod_postway_to_customer { get; set; }
            public int cod_customer_to_mass { get; set; }
        }

        public class Height
        {
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Insurance
        {
            public bool enable { get; set; }
            public int max { get; set; }
            public int min { get; set; }
        }

        public class Length
        {
            public int max { get; set; }
            public int min { get; set; }
        }
    }
}