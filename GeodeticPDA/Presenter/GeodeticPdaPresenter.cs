using GeodeticPDA.Model;
using System;
using System.Collections;
using System.Diagnostics;

namespace GeodeticPDA.Presenter
{
    class GeodeticPdaPresenter : IDisposable
    {
        private GeodeticPdaSystem _system = new GeodeticPdaSystem();

        internal bool AddProperty(string idStr, string numberStr, string desc,
            string gps1LatitudeStr, string gps1LongitudeStr, string gps2LatitudeStr, string gps2LongitudeStr)
        {
            int? id = ToInt(idStr);
            int? number = ToInt(numberStr);
            double? gps1Latitude = ToDouble(gps1LatitudeStr);
            double? gps1Longitude = ToDouble(gps1LongitudeStr);
            double? gps2Latitude = ToDouble(gps2LatitudeStr);
            double? gps2Longitude = ToDouble(gps2LongitudeStr);

            if (id.HasValue && number.HasValue && gps1Latitude.HasValue && gps1Longitude.HasValue && gps2Latitude.HasValue && gps2Latitude.HasValue)
            {
                if (desc.Length > 20)
                {
                    desc = desc.Substring(0, 20);
                }
                Property newProperty = new Property(id.Value, number.Value, desc, new GpsCoordinates(gps1Latitude.Value, gps1Longitude.Value),
                    new GpsCoordinates(gps2Latitude.Value, gps2Longitude.Value));

                if (_system.AddProperty(newProperty))
                {
                    Trace.WriteLine($"Added property: {newProperty}");
                    return true;
                }
                Trace.WriteLine($"Property was not added: {newProperty}");
                return false;
            }
            Trace.WriteLine($"Property was not added - Invalid inputs.");
            return false;
        }

        internal Property FindProperty(string idStr)
        {
            int? id = ToInt(idStr);
            if (id.HasValue)
            {
                return _system.FindProperty(id.Value);
            }
            Trace.WriteLine("Invalid ID.");
            return null;
        }

        internal bool RemoveProperty(string idStr)
        {
            int? id = ToInt(idStr);
            if (id.HasValue)
            {
                if (_system.RemoveProperty(id.Value))
                {
                    Trace.WriteLine($"Removed property with Id: {idStr}.");
                    return true;
                }
                Trace.WriteLine($"Property was not removed - id: {idStr}");
                return false;
            }
            Trace.WriteLine("Invalid ID.");
            return false;
        }

        internal bool SaveChanges(int oldId, string newIdStr, string numberStr, string desc,
            string gps1LatitudeStr, string gps1LongitudeStr, string gps2LatitudeStr, string gps2LongitudeStr)
        {
            int? newId = ToInt(newIdStr);
            int? number = ToInt(numberStr);
            double? gps1Latitude = ToDouble(gps1LatitudeStr);
            double? gps1Longitude = ToDouble(gps1LongitudeStr);
            double? gps2Latitude = ToDouble(gps2LatitudeStr);
            double? gps2Longitude = ToDouble(gps2LongitudeStr);

            if (newId.HasValue && number.HasValue && gps1Latitude.HasValue && gps1Longitude.HasValue && gps2Latitude.HasValue && gps2Latitude.HasValue)
            {
                if (desc.Length > 20)
                {
                    desc = desc.Substring(0, 20);
                }
                Property newProperty = new Property(newId.Value, number.Value, desc, new GpsCoordinates(gps1Latitude.Value, gps1Longitude.Value),
                    new GpsCoordinates(gps2Latitude.Value, gps2Longitude.Value));

                if (_system.UpdateProperty(oldId, newProperty))
                {
                    Trace.WriteLine($"Updated property: {newProperty}");
                    return true;
                }
                Trace.WriteLine($"Poperty was not updated: {newProperty}");
                return false;
            }
            Trace.WriteLine($"Property was not updated - Invalid inputs.");
            return false;
        }

        internal void GenerateProperties(string countStr)
        {
            int? count = ToInt(countStr);
            if (count.HasValue)
            {
                _system.GenerateProperties(count.Value);
                Trace.WriteLine("Data generated.");
            }
            else Trace.WriteLine("Invalid number of properties.");
        }

        internal IEnumerable MainFileItems()
        {
            return _system.MainFileItems();
        }

        internal IEnumerable OverfillingFileItems()
        {
            return _system.OverfillingFileItems();
        }

        internal string GetManagingData()
        {
            return _system.GetManagingData();
        }

        private double? ToDouble(string str)
        {
            if (double.TryParse(str, out double result))
            {
                return result;
            }
            return null;
        }

        private int? ToInt(string str)
        {
            if (int.TryParse(str, out int result))
            {
                return result;
            }
            return null;
        }

        public void Dispose()
        {
            _system.Dispose();
        }

    }
}
