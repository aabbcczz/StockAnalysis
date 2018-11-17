namespace StockAnalysis.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public sealed class Country
    {
        private static Dictionary<string, Country> CreatedCountryObjects = new Dictionary<string, Country>();

        /// <summary>
        /// Region information 
        /// </summary>
        public RegionInfo RegionInformation { get; private set; }

        public static bool IsSameCountry(Country c1, Country c2)
        {
            if (c1 == null || c2 == null)
            {
                throw new ArgumentNullException();
            }

            return c1.RegionInformation.ThreeLetterISORegionName == c2.RegionInformation.ThreeLetterISORegionName;
        }

        private Country(string countryCode)
        {
            if (!IsCountryCodeValid(countryCode))
            {
                throw new ArgumentException($"{countryCode} is not valid country code defined in ISO 3166");
            }

            RegionInformation = new RegionInfo(countryCode);
        }

        private bool IsCountryCodeValid(string countryCode)
        {
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                    .Select(culture => new RegionInfo(culture.LCID))
                        .Any(region => string.Compare(region.TwoLetterISORegionName, countryCode, true) == 0);
        }

        /// <summary>
        /// Create Country object by ISO 3166 code
        /// </summary>
        /// <param name="countryCode">ISO 3166 code</param>
        /// <returns>country object if code is valid, otherwise exception will be thrown out</returns>
        public static Country CreateCountryByCode(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentNullException("countryCode");
            }

            var country = new Country(countryCode);
            string isoCode = country.RegionInformation.ThreeLetterISORegionName;

            lock (CreatedCountryObjects)
            {
                if (CreatedCountryObjects.ContainsKey(isoCode))
                {
                    return CreatedCountryObjects[isoCode];
                }
                else
                {
                    CreatedCountryObjects.Add(isoCode, country);

                    return country;
                }
            }
        }
    }
}
