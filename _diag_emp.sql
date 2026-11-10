SET NOCOUNT ON;
SELECT 'EMP' AS t, e.SysObjectID, e.AttName, e.AttSurname, e.LinCompany, e.AttIsDeleted, u.UserName
FROM TSql_Employee e JOIN AspNetUsers u ON u.Id = e.LinAspNetUsert
WHERE u.UserName LIKE '%godoy%' OR u.Email LIKE '%godoy%';
SELECT 'CO' AS t, SysObjectID, TextLabel, BitIsDeleted FROM TSql_Company ORDER BY SysObjectID;
