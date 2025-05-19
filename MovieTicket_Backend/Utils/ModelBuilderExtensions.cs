namespace MovieTicket_Backend.Utils
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using System.Globalization;
    using System.Text;

    public static class ModelBuilderExtensions
    {
        public static void UseSnakeCaseNames(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                // Tên bảng
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));

                // Tên cột
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }

                // Tên khóa chính
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName()));
                }

                // Tên khóa ngoại
                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()));
                }

                // Tên chỉ mục
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
                }
            }
        }

        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var builder = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var ch = name[i];
                if (char.IsUpper(ch))
                {
                    if (i > 0)
                        builder.Append('_');
                    builder.Append(char.ToLower(ch, CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }
    }

}
