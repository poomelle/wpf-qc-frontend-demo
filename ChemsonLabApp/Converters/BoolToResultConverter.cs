using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChemsonLabApp.Converters
{
    public class BoolToResultConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values[0];
            var batchName = values[1] as string;
            var torqueDiff = values[2];
            var fusionDiff = values[3];
            var torqueWarning = values[4];
            var fusionWarning = values[5];

            if (string.IsNullOrWhiteSpace(batchName) || batchName.Contains("W/U") || batchName.Contains("STD"))
            {
                return null;
            }

            else
            {
                if (result is bool res && torqueDiff is double td && fusionDiff is double fd && torqueWarning is double tw && fusionWarning is double fw)
                {
                    //return res ? "Pass" : "Fail";
                    if (res)
                    {
                        if (Math.Abs(td) <= tw && Math.Abs(fd) <= fw)
                        {
                            return "Pass";
                        }
                        else
                        {
                            return "Warning";
                        }
                    }
                    else
                    {
                        return "Fail";
                    }

                }
                return null;
            }


        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
