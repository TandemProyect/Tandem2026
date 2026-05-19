using DAL;
using System;

namespace Desing.Models
{
  public class GooglePlacesAddressBlockModel
  {
    public string Prefix { get; set; }
    public string MapElementId { get; set; }
    public string Title { get; set; }
    public string Place_Id { get; set; }
    public string Formatted_Address { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public string Street_Number { get; set; }
    public string Route { get; set; }
    public string Subpremise { get; set; }
    public string Locality { get; set; }
    public string Admin_Area_1 { get; set; }
    public string Admin_Area_2 { get; set; }
    public string Postal_Code { get; set; }
    public string Country_Code { get; set; }
    public string Country_Name { get; set; }
    public string Address_Components_Json { get; set; }

    public static GooglePlacesAddressBlockModel FromJobside(TSql_Jobside model, string prefix, string title)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("prefix required", nameof(prefix));

      var isLoc = string.Equals(prefix, "Loc", StringComparison.OrdinalIgnoreCase);
      var isBill = string.Equals(prefix, "Bill", StringComparison.OrdinalIgnoreCase);
      return new GooglePlacesAddressBlockModel
      {
        Prefix = prefix,
        MapElementId = prefix + "AddressMap",
        Title = title ?? "Dirección",
        Place_Id = isLoc ? model.Loc_Place_Id : model.Bill_Place_Id,
        Formatted_Address = isLoc ? model.Loc_Formatted_Address : model.Bill_Formatted_Address,
        Lat = isLoc ? model.Loc_Lat : model.Bill_Lat,
        Lng = isLoc ? model.Loc_Lng : model.Bill_Lng,
        Street_Number = isLoc ? model.Loc_Street_Number : model.Bill_Street_Number,
        Route = isLoc ? model.Loc_Route : model.Bill_Route,
        Subpremise = isLoc ? model.Loc_Subpremise : model.Bill_Subpremise,
        Locality = isLoc ? model.Loc_Locality : model.Bill_Locality,
        Admin_Area_1 = isLoc ? model.Loc_Admin_Area_1 : model.Bill_Admin_Area_1,
        Admin_Area_2 = isLoc ? model.Loc_Admin_Area_2 : model.Bill_Admin_Area_2,
        Postal_Code = isLoc ? model.Loc_Postal_Code : model.Bill_Postal_Code,
        Country_Code = isLoc ? model.Loc_Country_Code : model.Bill_Country_Code,
        Country_Name = isLoc ? model.Loc_Country_Name : model.Bill_Country_Name,
        Address_Components_Json = isLoc ? model.Loc_Address_Components_Json : model.Bill_Address_Components_Json
      };
    }

    /// <summary>
    /// Empresa: mismas columnas Loc_* que en Jobside para un único bloque de dirección.
    /// </summary>
    public static GooglePlacesAddressBlockModel FromCompany(TSql_Company model, string prefix, string title)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("prefix required", nameof(prefix));

      var isLoc = string.Equals(prefix, "Loc", StringComparison.OrdinalIgnoreCase);
      if (!isLoc)
        throw new ArgumentException("TSql_Company solo usa columnas Loc_*; el prefijo debe ser Loc.", nameof(prefix));

      return new GooglePlacesAddressBlockModel
      {
        Prefix = prefix,
        MapElementId = prefix + "AddressMap",
        Title = title ?? "Dirección",
        Place_Id = model.Loc_Place_Id,
        Formatted_Address = model.Loc_Formatted_Address,
        Lat = model.Loc_Lat,
        Lng = model.Loc_Lng,
        Street_Number = model.Loc_Street_Number,
        Route = model.Loc_Route,
        Subpremise = model.Loc_Subpremise,
        Locality = model.Loc_Locality,
        Admin_Area_1 = model.Loc_Admin_Area_1,
        Admin_Area_2 = model.Loc_Admin_Area_2,
        Postal_Code = model.Loc_Postal_Code,
        Country_Code = model.Loc_Country_Code,
        Country_Name = model.Loc_Country_Name,
        Address_Components_Json = model.Loc_Address_Components_Json
      };
    }

    /// <summary>
    /// Sede (TSql_Branch): mismas columnas Loc_* que en <see cref="TSql_Company"/>.
    /// </summary>
    /// <param name="prefix">Prefijo HTML (<c>Loc</c>, <c>BrLoc</c>, …). Los datos siempre leen/escriben columnas <c>Loc_*</c> de la sede.</param>
    public static GooglePlacesAddressBlockModel FromBranch(TSql_Branch model, string prefix, string title)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("prefix required", nameof(prefix));

      return new GooglePlacesAddressBlockModel
      {
        Prefix = prefix,
        MapElementId = prefix + "AddressMap",
        Title = title ?? "Dirección",
        Place_Id = model.Loc_Place_Id,
        Formatted_Address = model.Loc_Formatted_Address,
        Lat = model.Loc_Lat,
        Lng = model.Loc_Lng,
        Street_Number = model.Loc_Street_Number,
        Route = model.Loc_Route,
        Subpremise = model.Loc_Subpremise,
        Locality = model.Loc_Locality,
        Admin_Area_1 = model.Loc_Admin_Area_1,
        Admin_Area_2 = model.Loc_Admin_Area_2,
        Postal_Code = model.Loc_Postal_Code,
        Country_Code = model.Loc_Country_Code,
        Country_Name = model.Loc_Country_Name,
        Address_Components_Json = model.Loc_Address_Components_Json
      };
    }
  }
}
