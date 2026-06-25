using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class SeededCategoryData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "HexColor", "Icon", "IsArchived", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("19b3ccec-b173-4d75-b476-0808aa740a7f"), "#32ADE6", "book.fill", false, true, "Edukacja" },
                    { new Guid("44bfe6b9-6c7d-4b53-8aec-c66cf722a005"), "#8E8E93", "tag.fill", false, true, "Inne" },
                    { new Guid("551a65d9-cfac-402b-a9f1-22d242f36f9a"), "#5856D6", "house.fill", false, true, "Mieszkanie i rachunki" },
                    { new Guid("69d087e3-42ce-48e8-93d9-ba6f3af0ed4d"), "#FF3B30", "bag.fill", false, true, "Zakupy" },
                    { new Guid("6a894c27-f13c-4fb0-8885-93c3890aec4a"), "#FF9500", "fork.knife", false, true, "Restauracje" },
                    { new Guid("750a22e8-4105-403b-a895-a6f46efb767d"), "#AF52DE", "gamecontroller.fill", false, true, "Rozrywka" },
                    { new Guid("7d6cfb81-7e4c-4b4f-9fe6-f31afb8f2e28"), "#34C759", "cart.fill", false, true, "Jedzenie" },
                    { new Guid("d3530310-5b29-4557-90ac-65c382fbab3c"), "#FF2D55", "cross.case.fill", false, true, "Zdrowie" },
                    { new Guid("d5d42cd6-e4a5-49ed-9ab9-fb8c54eb7c32"), "#007AFF", "car.fill", false, true, "Transport" },
                    { new Guid("e0f98104-b4a1-4ab3-af15-d9b802575f2d"), "#5AC8FA", "repeat.circle.fill", false, true, "Subskrypcje" },
                    { new Guid("f786c317-4bd0-452d-8839-117f4a2b0c16"), "#32ADE6", "airplane", false, true, "Podróże" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("19b3ccec-b173-4d75-b476-0808aa740a7f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44bfe6b9-6c7d-4b53-8aec-c66cf722a005"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("551a65d9-cfac-402b-a9f1-22d242f36f9a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("69d087e3-42ce-48e8-93d9-ba6f3af0ed4d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6a894c27-f13c-4fb0-8885-93c3890aec4a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("750a22e8-4105-403b-a895-a6f46efb767d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7d6cfb81-7e4c-4b4f-9fe6-f31afb8f2e28"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d3530310-5b29-4557-90ac-65c382fbab3c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d5d42cd6-e4a5-49ed-9ab9-fb8c54eb7c32"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e0f98104-b4a1-4ab3-af15-d9b802575f2d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f786c317-4bd0-452d-8839-117f4a2b0c16"));
        }
    }
}
