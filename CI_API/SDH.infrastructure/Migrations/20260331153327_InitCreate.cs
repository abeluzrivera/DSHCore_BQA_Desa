using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDH.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "parametro");

            migrationBuilder.EnsureSchema(
                name: "operativo");

            migrationBuilder.EnsureSchema(
                name: "seguridad");

            migrationBuilder.CreateTable(
                name: "Tbl_Cat_Grupo",
                schema: "parametro",
                columns: table => new
                {
                    Id_Grupo_Catalogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Grupo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Es_Sistema = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETDATE()"),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Cat_Grupo", x => x.Id_Grupo_Catalogo);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Maest_Usuario",
                schema: "seguridad",
                columns: table => new
                {
                    Id_Usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo_Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Clave_Hash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre_Asesor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Rol_Sistema = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fecha_Ultimo_Acceso = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Esta_Activo = table.Column<bool>(type: "bit", nullable: false),
                    Esta_Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    Esta_Bloqueado = table.Column<bool>(type: "bit", nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Maest_Usuario", x => x.Id_Usuario);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Cat_Item",
                schema: "parametro",
                columns: table => new
                {
                    Id_Item_Catalogo = table.Column<int>(type: "int", nullable: false),
                    Id_Grupo_Catalogo = table.Column<int>(type: "int", nullable: false),
                    Codigo_Valor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Texto_Visual = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Orden_Visual = table.Column<int>(type: "int", nullable: false),
                    Esta_Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETDATE()"),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Cat_Item", x => x.Id_Item_Catalogo);
                    table.ForeignKey(
                        name: "FK_Grupo_Item",
                        column: x => x.Id_Grupo_Catalogo,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Grupo",
                        principalColumn: "Id_Grupo_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Maest_Cliente",
                schema: "operativo",
                columns: table => new
                {
                    Id_Cliente = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identificacion_Cliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Id_Tipo_Identificacion = table.Column<int>(type: "int", nullable: false),
                    Nombre_Completo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Esta_Verificado = table.Column<bool>(type: "bit", nullable: false),
                    Esta_Aprobado = table.Column<bool>(type: "bit", nullable: false),
                    Esta_Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    Esta_Anonimizado = table.Column<bool>(type: "bit", nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fecha_Expiracion_Legal = table.Column<DateTime>(type: "Date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Maest_Cliente", x => x.Id_Cliente);
                    table.ForeignKey(
                        name: "FK_Cliente_TipoIdentificacion",
                        column: x => x.Id_Tipo_Identificacion,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Contacto_Cliente",
                schema: "operativo",
                columns: table => new
                {
                    Id_Contacto_Cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Cliente = table.Column<long>(type: "bigint", nullable: false),
                    Id_Tipo_Contacto = table.Column<int>(type: "int", nullable: false),
                    Valor_Contacto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id_Estado_Verificacion = table.Column<int>(type: "int", nullable: false),
                    Usuario_Verificador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Verificacion = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Esta_Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id_Estado_LOPDP = table.Column<int>(type: "int", nullable: false),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Contacto_Cliente", x => x.Id_Contacto_Cliente);
                    table.ForeignKey(
                        name: "FK_Cliente_Contacto",
                        column: x => x.Id_Cliente,
                        principalSchema: "operativo",
                        principalTable: "Tbl_Maest_Cliente",
                        principalColumn: "Id_Cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacto_Cliente_EstadoLOPDP",
                        column: x => x.Id_Estado_LOPDP,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacto_Cliente_EstadoVerificado",
                        column: x => x.Id_Estado_Verificacion,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacto_Cliente_TipoMedioContacto",
                        column: x => x.Id_Tipo_Contacto,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Direccion_Cliente",
                schema: "operativo",
                columns: table => new
                {
                    Id_Direccion_Cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Cliente = table.Column<long>(type: "bigint", nullable: false),
                    Id_Tipo_Direccion = table.Column<int>(type: "int", nullable: true),
                    Direccion_Completa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Codigo_Postal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Codigo_Pais = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Codigo_Ciudad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Codigo_Provincia = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Codigo_Parroquia = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Parroquia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitud = table.Column<decimal>(type: "decimal(18,10)", precision: 18, scale: 10, nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(18,10)", precision: 18, scale: 10, nullable: true),
                    Es_Principal = table.Column<bool>(type: "bit", nullable: true),
                    Esta_Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    Source_Direccion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado_Verificacion = table.Column<int>(type: "int", nullable: true),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Modificacion = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Usuario_Modificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Usuario_Verificador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Verificacion = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Usuario_Aprobador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fecha_Aprobacion = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    estado_LOPDP = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Direccion_Cliente", x => x.Id_Direccion_Cliente);
                    table.ForeignKey(
                        name: "FK_Cliente_Direccion",
                        column: x => x.Id_Cliente,
                        principalSchema: "operativo",
                        principalTable: "Tbl_Maest_Cliente",
                        principalColumn: "Id_Cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Direccion_Cliente_Estado_verificacion",
                        column: x => x.Estado_Verificacion,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Direccion_Cliente_Tipo_Direccion",
                        column: x => x.Id_Tipo_Direccion,
                        principalSchema: "parametro",
                        principalTable: "Tbl_Cat_Item",
                        principalColumn: "Id_Item_Catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Financiero_cliente",
                schema: "operativo",
                columns: table => new
                {
                    Id_Financiero_Cliente = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Cliente = table.Column<long>(type: "bigint", nullable: false),
                    Tipo_Contabilidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Monto_Contable = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Financiero_cliente", x => x.Id_Financiero_Cliente);
                    table.ForeignKey(
                        name: "FK_Cliente_Financiero",
                        column: x => x.Id_Cliente,
                        principalSchema: "operativo",
                        principalTable: "Tbl_Maest_Cliente",
                        principalColumn: "Id_Cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_Oficializacion_Core",
                schema: "operativo",
                columns: table => new
                {
                    Id_Oficializacion_Core = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Cliente = table.Column<long>(type: "bigint", nullable: false),
                    Trama_Json_Enviada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Respuesta_Core_Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    Usuario_Creacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Oficializacion_Core", x => x.Id_Oficializacion_Core);
                    table.ForeignKey(
                        name: "FK_Cliente_Oficializacion",
                        column: x => x.Id_Cliente,
                        principalSchema: "operativo",
                        principalTable: "Tbl_Maest_Cliente",
                        principalColumn: "Id_Cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_GrupoCatalogo_NombreGrupo",
                schema: "parametro",
                table: "Tbl_Cat_Grupo",
                column: "Nombre_Grupo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCatalogo_Buscador",
                schema: "parametro",
                table: "Tbl_Cat_Item",
                columns: new[] { "Id_Grupo_Catalogo", "Esta_Activo" })
                .Annotation("SqlServer:Include", new[] { "Codigo_Valor", "Texto_Visual", "Orden_Visual" });

            migrationBuilder.CreateIndex(
                name: "UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor",
                schema: "parametro",
                table: "Tbl_Cat_Item",
                columns: new[] { "Id_Grupo_Catalogo", "Codigo_Valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Cliente_Performance_Query",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                columns: new[] { "Id_Cliente", "Id_Estado_Verificacion", "Id_Tipo_Contacto" },
                filter: "[Esta_Eliminado] = 0")
                .Annotation("SqlServer:Include", new[] { "Valor_Contacto", "Esta_Eliminado" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_Cliente_Valor",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                column: "Valor_Contacto");

            migrationBuilder.CreateIndex(
                name: "IX_ContactoCliente_EstadoVerificacion",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                column: "Id_Estado_Verificacion");

            migrationBuilder.CreateIndex(
                name: "IX_ContactoCliente_IdCliente_EstaEliminado",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                columns: new[] { "Id_Cliente", "Esta_Eliminado" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                column: "Id_Estado_LOPDP");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto",
                schema: "operativo",
                table: "Tbl_Contacto_Cliente",
                column: "Id_Tipo_Contacto");

            migrationBuilder.CreateIndex(
                name: "IX_ContactoCliente_IdCliente_EstaEliminado",
                schema: "operativo",
                table: "Tbl_Direccion_Cliente",
                columns: new[] { "Id_Cliente", "Esta_Eliminado" });

            migrationBuilder.CreateIndex(
                name: "IX_Direccion_Cliente_IdCliente",
                schema: "operativo",
                table: "Tbl_Direccion_Cliente",
                columns: new[] { "Id_Direccion_Cliente", "Es_Principal" })
                .Annotation("SqlServer:Include", new[] { "Direccion_Completa", "Ciudad", "Latitud", "Longitud" });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Direccion_Cliente_Estado_Verificacion",
                schema: "operativo",
                table: "Tbl_Direccion_Cliente",
                column: "Estado_Verificacion");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion",
                schema: "operativo",
                table: "Tbl_Direccion_Cliente",
                column: "Id_Tipo_Direccion");

            migrationBuilder.CreateIndex(
                name: "IX_FinancieroCliente_IdCliente",
                schema: "operativo",
                table: "Tbl_Financiero_cliente",
                column: "Id_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_FinancieroCliente_TipoContabilidad",
                schema: "operativo",
                table: "Tbl_Financiero_cliente",
                column: "Tipo_Contabilidad");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Identificacion",
                schema: "operativo",
                table: "Tbl_Maest_Cliente",
                column: "Identificacion_Cliente",
                unique: true,
                filter: "[Esta_Eliminado] = 0 AND [Esta_Anonimizado] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Nombre",
                schema: "operativo",
                table: "Tbl_Maest_Cliente",
                column: "Nombre_Completo",
                filter: "[Esta_Eliminado] = 0 AND [Esta_Anonimizado] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion",
                schema: "operativo",
                table: "Tbl_Maest_Cliente",
                column: "Id_Tipo_Identificacion");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CodigoUsuario",
                schema: "seguridad",
                table: "Tbl_Maest_Usuario",
                column: "Codigo_Usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                schema: "seguridad",
                table: "Tbl_Maest_Usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_EstaActivo",
                schema: "seguridad",
                table: "Tbl_Maest_Usuario",
                column: "Esta_Activo");

            migrationBuilder.CreateIndex(
                name: "IX_OficializacionCore_FechaCreacion",
                schema: "operativo",
                table: "Tbl_Oficializacion_Core",
                column: "Fecha_Creacion");

            migrationBuilder.CreateIndex(
                name: "IX_OficializacionCore_IdCliente",
                schema: "operativo",
                table: "Tbl_Oficializacion_Core",
                column: "Id_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_OficializacionCore_RespuestaCoreCodigo",
                schema: "operativo",
                table: "Tbl_Oficializacion_Core",
                column: "Respuesta_Core_Codigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_Contacto_Cliente",
                schema: "operativo");

            migrationBuilder.DropTable(
                name: "Tbl_Direccion_Cliente",
                schema: "operativo");

            migrationBuilder.DropTable(
                name: "Tbl_Financiero_cliente",
                schema: "operativo");

            migrationBuilder.DropTable(
                name: "Tbl_Maest_Usuario",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "Tbl_Oficializacion_Core",
                schema: "operativo");

            migrationBuilder.DropTable(
                name: "Tbl_Maest_Cliente",
                schema: "operativo");

            migrationBuilder.DropTable(
                name: "Tbl_Cat_Item",
                schema: "parametro");

            migrationBuilder.DropTable(
                name: "Tbl_Cat_Grupo",
                schema: "parametro");
        }
    }
}
