using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Roles.Queries.GetAllPermissions
{
    public sealed class GetAllPermissionsHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionGroupResponse>>>
    {
        public async Task<Result<List<PermissionGroupResponse>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissionGroups = new List<PermissionGroupResponse>();

            // 1. AppPermissions içindeki tüm alt sınıfları (Products, Invoices vb.) al
            var nestedClasses = typeof(AppPermissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var nestedClass in nestedClasses)
            {
                // 2. Alt sınıfın içindeki tüm 'public const string' alanları bul
                var permissions = nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                    .Select(fi => fi.GetRawConstantValue()?.ToString() ?? "")
                    .ToList();

                // 3. Modül adıyla birlikte listeye ekle
                permissionGroups.Add(new PermissionGroupResponse(nestedClass.Name, permissions));
            }

            return await Task.FromResult(Result<List<PermissionGroupResponse>>.Success(permissionGroups,"Yetkiler başarıyla çekildi."));
        }
    }
}
