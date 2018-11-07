using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Static_classes
{
    public static class ComboboxValues
    {

        public static readonly object[] AGGREGATE_SIZE =
        {
            "8",
            "12",
            "16",
            "32"
        };

        public static readonly object[] CONCRETE_STRENGTH_CLASSES = {
            "C20/25",
            "C25/30",
            "C30/37",
            "C35/45",
            "C40/50",
            "C45/55",
            "C50/60",
            "C55/65",
            "C60/70",
            "C70/80",
            "C80/90",
            "C90/100",
            "Custom"
        };

        public static readonly object[] STEEL_DIAMETER =
        {
            "6",
            "8",
            "10",
            "12",
            "16",
            "20",
            "25",
            "32"
        };

        public static readonly object[] REINF_STRENGTH_CLASSES =
        {
            "B500B",
            "Custom"
        };

        public static readonly object[] STEEL_STRENGTH_CLASSES =
        {
            "S235",
            "S355",
            "S450",
            "S550",
            "Custom"
                
        };

        public static readonly object[] EXPOSURE_CLASSES =
        {
            "X0",
            "XC1",
            "XC2",
            "XC3",
            "XC4",
            "XD1",
            "XD2",
            "XD3",
            "XS1",
            "XS2",
            "XS3"
        };

        public static readonly object[] DES_WORK_LIFE =
        {
            "50 years",
            "100 years"
        };

        public static readonly object[] RH =
        {
            "40%",
            "70%",
            "90%",
            "100%"

        };

    }
}
