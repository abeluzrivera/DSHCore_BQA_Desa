USE [DB_ODS]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] DROP CONSTRAINT [FK_Grupo_Item]
GO
ALTER TABLE [operativo].[Tbl_Oficializacion_Core] DROP CONSTRAINT [FK_Cliente_Oficializacion]
GO
ALTER TABLE [operativo].[Tbl_Maest_Cliente] DROP CONSTRAINT [FK_Cliente_TipoIdentificacion]
GO
ALTER TABLE [operativo].[Tbl_Financiero_cliente] DROP CONSTRAINT [FK_Cliente_Financiero]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] DROP CONSTRAINT [FK_Direccion_Cliente_Tipo_Direccion]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] DROP CONSTRAINT [FK_Direccion_Cliente_Estado_verificacion]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] DROP CONSTRAINT [FK_Cliente_Direccion]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] DROP CONSTRAINT [FK_Contacto_Cliente_TipoMedioContacto]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] DROP CONSTRAINT [FK_Contacto_Cliente_EstadoVerificado]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] DROP CONSTRAINT [FK_Contacto_Cliente_EstadoLOPDP]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] DROP CONSTRAINT [FK_Cliente_Contacto]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] DROP CONSTRAINT [DF__Tbl_Cat_I__Fecha__3F466844]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] DROP CONSTRAINT [DF__Tbl_Cat_I__Esta___3E52440B]
GO
ALTER TABLE [parametro].[Tbl_Cat_Grupo] DROP CONSTRAINT [DF__Tbl_Cat_G__Fecha__398D8EEE]
GO
ALTER TABLE [parametro].[Tbl_Cat_Grupo] DROP CONSTRAINT [DF__Tbl_Cat_G__Es_Si__38996AB5]
GO
/****** Objeto: Index [IX_Usuario_EstaActivo] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Usuario_EstaActivo] ON [seguridad].[Tbl_Maest_Usuario]
GO
/****** Objeto: Index [IX_Usuario_Email] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Usuario_Email] ON [seguridad].[Tbl_Maest_Usuario]
GO
/****** Objeto: Index [IX_Usuario_CodigoUsuario] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Usuario_CodigoUsuario] ON [seguridad].[Tbl_Maest_Usuario]
GO
/****** Objeto: Index [UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor] ON [parametro].[Tbl_Cat_Item]
GO
/****** Objeto: Index [IX_ItemCatalogo_Buscador] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_ItemCatalogo_Buscador] ON [parametro].[Tbl_Cat_Item]
GO
/****** Objeto: Index [UQ_GrupoCatalogo_NombreGrupo] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [UQ_GrupoCatalogo_NombreGrupo] ON [parametro].[Tbl_Cat_Grupo]
GO
/****** Objeto: Index [IX_OficializacionCore_RespuestaCoreCodigo] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_OficializacionCore_RespuestaCoreCodigo] ON [operativo].[Tbl_Oficializacion_Core]
GO
/****** Objeto: Index [IX_OficializacionCore_IdCliente] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_OficializacionCore_IdCliente] ON [operativo].[Tbl_Oficializacion_Core]
GO
/****** Objeto: Index [IX_OficializacionCore_FechaCreacion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_OficializacionCore_FechaCreacion] ON [operativo].[Tbl_Oficializacion_Core]
GO
/****** Objeto: Index [IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
GO
/****** Objeto: Index [IX_Cliente_Nombre] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Cliente_Nombre] ON [operativo].[Tbl_Maest_Cliente]
GO
/****** Objeto: Index [IX_Cliente_Identificacion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Cliente_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
GO
/****** Objeto: Index [IX_FinancieroCliente_TipoContabilidad] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_FinancieroCliente_TipoContabilidad] ON [operativo].[Tbl_Financiero_cliente]
GO
/****** Objeto: Index [IX_FinancieroCliente_IdCliente] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_FinancieroCliente_IdCliente] ON [operativo].[Tbl_Financiero_cliente]
GO
/****** Objeto: Index [IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion] ON [operativo].[Tbl_Direccion_Cliente]
GO
/****** Objeto: Index [IX_Tbl_Direccion_Cliente_Estado_Verificacion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Tbl_Direccion_Cliente_Estado_Verificacion] ON [operativo].[Tbl_Direccion_Cliente]
GO
/****** Objeto: Index [IX_Direccion_Cliente_IdCliente] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Direccion_Cliente_IdCliente] ON [operativo].[Tbl_Direccion_Cliente]
GO
/****** Objeto: Index [IX_ContactoCliente_IdCliente_EstaEliminado] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_ContactoCliente_IdCliente_EstaEliminado] ON [operativo].[Tbl_Direccion_Cliente]
GO
/****** Objeto: Index [IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Index [IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Index [IX_ContactoCliente_IdCliente_EstaEliminado] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_ContactoCliente_IdCliente_EstaEliminado] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Index [IX_ContactoCliente_EstadoVerificacion] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_ContactoCliente_EstadoVerificacion] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Index [IX_Contacto_Cliente_Valor] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Contacto_Cliente_Valor] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Index [IX_Contacto_Cliente_Performance_Query] Fecha de script: 10/6/2026 10:47:56 ******/
DROP INDEX [IX_Contacto_Cliente_Performance_Query] ON [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Table [seguridad].[Tbl_Maest_Usuario] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[seguridad].[Tbl_Maest_Usuario]') AND type in (N'U'))
DROP TABLE [seguridad].[Tbl_Maest_Usuario]
GO
/****** Objeto: Table [operativo].[Tbl_Oficializacion_Core] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[operativo].[Tbl_Oficializacion_Core]') AND type in (N'U'))
DROP TABLE [operativo].[Tbl_Oficializacion_Core]
GO
/****** Objeto: Table [operativo].[Tbl_Financiero_cliente] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[operativo].[Tbl_Financiero_cliente]') AND type in (N'U'))
DROP TABLE [operativo].[Tbl_Financiero_cliente]
GO
/****** Objeto: Table [dbo].[__EFMigrationsHistory] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
DROP TABLE [dbo].[__EFMigrationsHistory]
GO
/****** Objeto: View [operativo].[vw_Clientes_Contactos_Detalle] Fecha de script: 10/6/2026 10:47:56 ******/
DROP VIEW [operativo].[vw_Clientes_Contactos_Detalle]
GO
/****** Objeto: Table [operativo].[Tbl_Direccion_Cliente] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[operativo].[Tbl_Direccion_Cliente]') AND type in (N'U'))
DROP TABLE [operativo].[Tbl_Direccion_Cliente]
GO
/****** Objeto: Table [operativo].[Tbl_Contacto_Cliente] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[operativo].[Tbl_Contacto_Cliente]') AND type in (N'U'))
DROP TABLE [operativo].[Tbl_Contacto_Cliente]
GO
/****** Objeto: Table [operativo].[Tbl_Maest_Cliente] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[operativo].[Tbl_Maest_Cliente]') AND type in (N'U'))
DROP TABLE [operativo].[Tbl_Maest_Cliente]
GO
/****** Objeto: View [parametro].[Vw_Cat_Detalle_General] Fecha de script: 10/6/2026 10:47:56 ******/
DROP VIEW [parametro].[Vw_Cat_Detalle_General]
GO
/****** Objeto: Table [parametro].[Tbl_Cat_Item] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[parametro].[Tbl_Cat_Item]') AND type in (N'U'))
DROP TABLE [parametro].[Tbl_Cat_Item]
GO
/****** Objeto: Table [parametro].[Tbl_Cat_Grupo] Fecha de script: 10/6/2026 10:47:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[parametro].[Tbl_Cat_Grupo]') AND type in (N'U'))
DROP TABLE [parametro].[Tbl_Cat_Grupo]
GO
/****** Objeto: Schema [seguridad] Fecha de script: 10/6/2026 10:47:56 ******/
DROP SCHEMA [seguridad]
GO
/****** Objeto: Schema [parametro] Fecha de script: 10/6/2026 10:47:56 ******/
DROP SCHEMA [parametro]
GO
/****** Objeto: Schema [operativo] Fecha de script: 10/6/2026 10:47:56 ******/
DROP SCHEMA [operativo]
GO
/****** Objeto: User [usr_ods] Fecha de script: 10/6/2026 10:47:56 ******/
DROP USER [usr_ods]
GO
USE [master]
GO
/****** Objeto: Database [DB_ODS] Fecha de script: 10/6/2026 10:47:56 ******/
DROP DATABASE [DB_ODS]
GO
/****** Objeto: Database [DB_ODS] Fecha de script: 10/6/2026 10:47:56 ******/
CREATE DATABASE [DB_ODS]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DB_ODS', FILENAME = N'E:\SQL\Data\DB_ODS.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DB_ODS_log', FILENAME = N'L:\SQL\Log\DB_ODS_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [DB_ODS] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DB_ODS].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DB_ODS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DB_ODS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DB_ODS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DB_ODS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DB_ODS] SET ARITHABORT OFF 
GO
ALTER DATABASE [DB_ODS] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DB_ODS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DB_ODS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DB_ODS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DB_ODS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DB_ODS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DB_ODS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DB_ODS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DB_ODS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DB_ODS] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DB_ODS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DB_ODS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DB_ODS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DB_ODS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DB_ODS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DB_ODS] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [DB_ODS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DB_ODS] SET RECOVERY FULL 
GO
ALTER DATABASE [DB_ODS] SET  MULTI_USER 
GO
ALTER DATABASE [DB_ODS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DB_ODS] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DB_ODS] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DB_ODS] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DB_ODS] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DB_ODS] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [DB_ODS] SET QUERY_STORE = OFF
GO
ALTER AUTHORIZATION ON DATABASE::[DB_ODS] TO [BANCOMACHALA\pedro.rivera]
GO
USE [DB_ODS]
GO
/****** Objeto: User [usr_ods] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE USER [usr_ods] FOR LOGIN [usr_ods] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [usr_ods]
GO
ALTER ROLE [db_datareader] ADD MEMBER [usr_ods]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [usr_ods]
GO
/****** Objeto: Schema [operativo] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE SCHEMA [operativo] AUTHORIZATION [dbo]
GO
/****** Objeto: Schema [parametro] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE SCHEMA [parametro] AUTHORIZATION [dbo]
GO
/****** Objeto: Schema [seguridad] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE SCHEMA [seguridad] AUTHORIZATION [dbo]
GO
/****** Objeto: Table [parametro].[Tbl_Cat_Grupo] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [parametro].[Tbl_Cat_Grupo](
	[Id_Grupo_Catalogo] [int] IDENTITY(1,1) NOT NULL,
	[Nombre_Grupo] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
	[Es_Sistema] [bit] NOT NULL,
	[Fecha_Creacion] [datetime2](3) NOT NULL,
	[Usuario_Creacion] [nvarchar](50) NULL,
	[Fecha_Modificacion] [datetime2](3) NULL,
	[Usuario_Modificacion] [nvarchar](50) NULL,
 CONSTRAINT [PK_Tbl_Cat_Grupo] PRIMARY KEY CLUSTERED 
(
	[Id_Grupo_Catalogo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [parametro].[Tbl_Cat_Grupo] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [parametro].[Tbl_Cat_Item] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [parametro].[Tbl_Cat_Item](
	[Id_Item_Catalogo] [int] NOT NULL,
	[Id_Grupo_Catalogo] [int] NOT NULL,
	[Codigo_Valor] [nvarchar](20) NOT NULL,
	[Texto_Visual] [nvarchar](100) NOT NULL,
	[Orden_Visual] [int] NOT NULL,
	[Esta_Activo] [bit] NOT NULL,
	[Fecha_Creacion] [datetime2](3) NOT NULL,
	[Usuario_Creacion] [nvarchar](50) NULL,
	[Fecha_Modificacion] [datetime2](3) NULL,
	[Usuario_Modificacion] [nvarchar](50) NULL,
 CONSTRAINT [PK_Tbl_Cat_Item] PRIMARY KEY CLUSTERED 
(
	[Id_Item_Catalogo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [parametro].[Tbl_Cat_Item] TO  SCHEMA OWNER 
GO
/****** Objeto: View [parametro].[Vw_Cat_Detalle_General] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

                CREATE   VIEW [parametro].[Vw_Cat_Detalle_General]
                AS
                SELECT
                    i.Id_Item_Catalogo,
                    g.Id_Grupo_Catalogo,
                    g.Nombre_Grupo,
                    i.Codigo_Valor,
                    i.Texto_Visual,
                    i.Orden_Visual,
                    i.Esta_Activo,
                    g.Es_Sistema
                FROM parametro.Tbl_Cat_Item  AS i
                JOIN parametro.Tbl_Cat_Grupo AS g ON g.Id_Grupo_Catalogo = i.Id_Grupo_Catalogo;
            
GO
ALTER AUTHORIZATION ON [parametro].[Vw_Cat_Detalle_General] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [operativo].[Tbl_Maest_Cliente] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operativo].[Tbl_Maest_Cliente](
	[Id_Cliente] [bigint] IDENTITY(1,1) NOT NULL,
	[Identificacion_Cliente] [nvarchar](20) NOT NULL,
	[Id_Tipo_Identificacion] [int] NOT NULL,
	[Nombre_Completo] [nvarchar](250) NULL,
	[Esta_Verificado] [bit] NOT NULL,
	[Esta_Aprobado] [bit] NOT NULL,
	[Esta_Eliminado] [bit] NOT NULL,
	[Esta_Anonimizado] [bit] NOT NULL,
	[Fecha_Creacion] [datetime2](3) NOT NULL,
	[Usuario_Creacion] [nvarchar](100) NOT NULL,
	[Fecha_Modificacion] [datetime2](3) NULL,
	[Usuario_Modificacion] [nvarchar](100) NULL,
	[Fecha_Expiracion_Legal] [date] NULL,
 CONSTRAINT [PK_Tbl_Maest_Cliente] PRIMARY KEY CLUSTERED 
(
	[Id_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [operativo].[Tbl_Maest_Cliente] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [operativo].[Tbl_Contacto_Cliente] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operativo].[Tbl_Contacto_Cliente](
	[Id_Contacto_Cliente] [int] IDENTITY(1,1) NOT NULL,
	[Id_Cliente] [bigint] NOT NULL,
	[Id_Tipo_Contacto] [int] NOT NULL,
	[Valor_Contacto] [nvarchar](50) NOT NULL,
	[Id_Estado_Verificacion] [int] NOT NULL,
	[Usuario_Verificador] [nvarchar](50) NULL,
	[Fecha_Verificacion] [datetime2](3) NULL,
	[Fecha_Creacion] [datetime2](3) NOT NULL,
	[Usuario_Creacion] [nvarchar](100) NOT NULL,
	[Esta_Eliminado] [bit] NOT NULL,
	[Source] [nvarchar](50) NOT NULL,
	[Id_Estado_LOPDP] [int] NOT NULL,
	[Usuario_Modificacion] [nvarchar](100) NULL,
	[Fecha_Modificacion] [datetime2](3) NULL,
 CONSTRAINT [PK_Tbl_Contacto_Cliente] PRIMARY KEY CLUSTERED 
(
	[Id_Contacto_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [operativo].[Tbl_Contacto_Cliente] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [operativo].[Tbl_Direccion_Cliente] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operativo].[Tbl_Direccion_Cliente](
	[Id_Direccion_Cliente] [int] IDENTITY(1,1) NOT NULL,
	[Id_Cliente] [bigint] NOT NULL,
	[Id_Tipo_Direccion] [int] NULL,
	[Direccion_Completa] [nvarchar](500) NULL,
	[Ciudad] [nvarchar](100) NULL,
	[Provincia] [nvarchar](100) NULL,
	[Codigo_Postal] [nvarchar](20) NULL,
	[Pais] [nvarchar](100) NULL,
	[Codigo_Pais] [nvarchar](10) NULL,
	[Codigo_Ciudad] [nvarchar](10) NULL,
	[Codigo_Provincia] [nvarchar](10) NULL,
	[Codigo_Parroquia] [nvarchar](10) NULL,
	[Parroquia] [nvarchar](100) NULL,
	[Latitud] [decimal](18, 10) NULL,
	[Longitud] [decimal](18, 10) NULL,
	[Es_Principal] [bit] NULL,
	[Esta_Eliminado] [bit] NOT NULL,
	[Source_Direccion] [nvarchar](50) NULL,
	[Estado_Verificacion] [int] NULL,
	[Fecha_Creacion] [datetime2](7) NULL,
	[Usuario_Creacion] [nvarchar](50) NULL,
	[Fecha_Modificacion] [datetime2](7) NULL,
	[Usuario_Modificacion] [nvarchar](50) NULL,
	[Usuario_Verificador] [nvarchar](50) NULL,
	[Fecha_Verificacion] [datetime2](7) NULL,
	[Usuario_Aprobador] [nvarchar](50) NULL,
	[Fecha_Aprobacion] [datetime2](7) NULL,
	[estado_LOPDP] [nvarchar](20) NULL,
 CONSTRAINT [PK_Tbl_Direccion_Cliente] PRIMARY KEY CLUSTERED 
(
	[Id_Direccion_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [operativo].[Tbl_Direccion_Cliente] TO  SCHEMA OWNER 
GO
/****** Objeto: View [operativo].[vw_Clientes_Contactos_Detalle] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

                CREATE   VIEW [operativo].[vw_Clientes_Contactos_Detalle] 
                AS
                SELECT 
                    Mae.Identificacion_Cliente AS [Identificacion Cliente], 
                    Mae.Nombre_Completo AS [Nombre Completo], 
                    Cel.Valor_Contacto AS Celular, 
                    Cel.Source AS Fuente_Celular,
                    Cor.Valor_Contacto AS Correo,
                    Cor.Source AS Fuente_Correo,
                    Con.Valor_Contacto AS Telefono_Convencional,
                    Con.Source AS Fuente_Telefono_Convencional,
                    Dir_H.Direccion_Completa AS Direccion_Domicilio,
                    Dir_H.Source_Direccion AS Fuente_Direccion_Domicilio,
                    Dir_T.Direccion_Completa AS Direccion_Trabajo,
                    Dir_H.Source_Direccion AS Fuente_Direccion_Trabajo
                FROM operativo.Tbl_Maest_Cliente AS Mae
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Cel 
                    ON Mae.Id_Cliente = Cel.Id_Cliente AND Cel.Id_Tipo_Contacto = 101
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Cor
                    ON Mae.Id_Cliente = Cor.Id_Cliente AND Cor.Id_Tipo_Contacto = 102
                LEFT JOIN operativo.Tbl_Contacto_Cliente AS Con
                    ON Mae.Id_Cliente = Con.Id_Cliente AND Con.Id_Tipo_Contacto = 103
                LEFT JOIN operativo.Tbl_Direccion_Cliente AS Dir_H
                    ON Mae.Id_Cliente = Dir_H.Id_Cliente AND Dir_H.Id_Tipo_Direccion = 105
                LEFT JOIN operativo.Tbl_Direccion_Cliente AS Dir_T
                    ON Mae.Id_Cliente = Dir_T.Id_Cliente AND Dir_T.Id_Tipo_Direccion = 106;
            
GO
ALTER AUTHORIZATION ON [operativo].[vw_Clientes_Contactos_Detalle] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [dbo].[__EFMigrationsHistory] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [dbo].[__EFMigrationsHistory] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [operativo].[Tbl_Financiero_cliente] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operativo].[Tbl_Financiero_cliente](
	[Id_Financiero_Cliente] [bigint] IDENTITY(1,1) NOT NULL,
	[Id_Cliente] [bigint] NOT NULL,
	[Tipo_Contabilidad] [nvarchar](30) NOT NULL,
	[Monto_Contable] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Tbl_Financiero_cliente] PRIMARY KEY CLUSTERED 
(
	[Id_Financiero_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [operativo].[Tbl_Financiero_cliente] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [operativo].[Tbl_Oficializacion_Core] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operativo].[Tbl_Oficializacion_Core](
	[Id_Oficializacion_Core] [bigint] IDENTITY(1,1) NOT NULL,
	[Id_Cliente] [bigint] NOT NULL,
	[Trama_Json_Enviada] [nvarchar](max) NOT NULL,
	[Respuesta_Core_Codigo] [nvarchar](10) NOT NULL,
	[Fecha_Creacion] [datetime2](7) NOT NULL,
	[Usuario_Creacion] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Tbl_Oficializacion_Core] PRIMARY KEY CLUSTERED 
(
	[Id_Oficializacion_Core] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [operativo].[Tbl_Oficializacion_Core] TO  SCHEMA OWNER 
GO
/****** Objeto: Table [seguridad].[Tbl_Maest_Usuario] Fecha de script: 10/6/2026 10:47:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seguridad].[Tbl_Maest_Usuario](
	[Id_Usuario] [int] IDENTITY(1,1) NOT NULL,
	[Codigo_Usuario] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Clave_Hash] [nvarchar](200) NOT NULL,
	[Nombre_Asesor] [nvarchar](150) NOT NULL,
	[Rol_Sistema] [nvarchar](50) NOT NULL,
	[Fecha_Ultimo_Acceso] [datetime2](7) NULL,
	[Esta_Activo] [bit] NOT NULL,
	[Esta_Eliminado] [bit] NOT NULL,
	[Esta_Bloqueado] [bit] NULL,
	[Fecha_Creacion] [datetime2](7) NOT NULL,
	[Usuario_Creacion] [nvarchar](50) NULL,
	[Fecha_Modificacion] [datetime2](7) NULL,
	[Usuario_Modificacion] [nvarchar](50) NULL,
 CONSTRAINT [PK_Tbl_Maest_Usuario] PRIMARY KEY CLUSTERED 
(
	[Id_Usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER AUTHORIZATION ON [seguridad].[Tbl_Maest_Usuario] TO  SCHEMA OWNER 
GO
/****** Objeto: Index [IX_Contacto_Cliente_Performance_Query] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Contacto_Cliente_Performance_Query] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Cliente] ASC,
	[Id_Estado_Verificacion] ASC,
	[Id_Tipo_Contacto] ASC
)
INCLUDE([Valor_Contacto],[Esta_Eliminado]) 
WHERE ([Esta_Eliminado]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Contacto_Cliente_Valor] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Contacto_Cliente_Valor] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Valor_Contacto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_ContactoCliente_EstadoVerificacion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_ContactoCliente_EstadoVerificacion] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Estado_Verificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_ContactoCliente_IdCliente_EstaEliminado] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_ContactoCliente_IdCliente_EstaEliminado] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Cliente] ASC,
	[Esta_Eliminado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Estado_LOPDP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Tipo_Contacto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_ContactoCliente_IdCliente_EstaEliminado] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_ContactoCliente_IdCliente_EstaEliminado] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Cliente] ASC,
	[Esta_Eliminado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Direccion_Cliente_IdCliente] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Direccion_Cliente_IdCliente] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Direccion_Cliente] ASC,
	[Es_Principal] ASC
)
INCLUDE([Direccion_Completa],[Ciudad],[Latitud],[Longitud]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Tbl_Direccion_Cliente_Estado_Verificacion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Tbl_Direccion_Cliente_Estado_Verificacion] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Estado_Verificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Tipo_Direccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_FinancieroCliente_IdCliente] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_FinancieroCliente_IdCliente] ON [operativo].[Tbl_Financiero_cliente]
(
	[Id_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_FinancieroCliente_TipoContabilidad] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_FinancieroCliente_TipoContabilidad] ON [operativo].[Tbl_Financiero_cliente]
(
	[Tipo_Contabilidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Cliente_Identificacion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cliente_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
(
	[Identificacion_Cliente] ASC
)
WHERE ([Esta_Eliminado]=(0) AND [Esta_Anonimizado]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Cliente_Nombre] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Cliente_Nombre] ON [operativo].[Tbl_Maest_Cliente]
(
	[Nombre_Completo] ASC
)
WHERE ([Esta_Eliminado]=(0) AND [Esta_Anonimizado]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
(
	[Id_Tipo_Identificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_OficializacionCore_FechaCreacion] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_OficializacionCore_FechaCreacion] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Fecha_Creacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_OficializacionCore_IdCliente] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_OficializacionCore_IdCliente] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Id_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_OficializacionCore_RespuestaCoreCodigo] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_OficializacionCore_RespuestaCoreCodigo] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Respuesta_Core_Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UQ_GrupoCatalogo_NombreGrupo] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_GrupoCatalogo_NombreGrupo] ON [parametro].[Tbl_Cat_Grupo]
(
	[Nombre_Grupo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_ItemCatalogo_Buscador] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_ItemCatalogo_Buscador] ON [parametro].[Tbl_Cat_Item]
(
	[Id_Grupo_Catalogo] ASC,
	[Esta_Activo] ASC
)
INCLUDE([Codigo_Valor],[Texto_Visual],[Orden_Visual]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor] ON [parametro].[Tbl_Cat_Item]
(
	[Id_Grupo_Catalogo] ASC,
	[Codigo_Valor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Usuario_CodigoUsuario] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usuario_CodigoUsuario] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Codigo_Usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Usuario_Email] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usuario_Email] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Usuario_EstaActivo] Fecha de script: 10/6/2026 10:47:57 ******/
CREATE NONCLUSTERED INDEX [IX_Usuario_EstaActivo] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Esta_Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [parametro].[Tbl_Cat_Grupo] ADD  DEFAULT (CONVERT([bit],(0))) FOR [Es_Sistema]
GO
ALTER TABLE [parametro].[Tbl_Cat_Grupo] ADD  DEFAULT (getdate()) FOR [Fecha_Creacion]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] ADD  DEFAULT (CONVERT([bit],(1))) FOR [Esta_Activo]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] ADD  DEFAULT (getdate()) FOR [Fecha_Creacion]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Contacto] FOREIGN KEY([Id_Cliente])
REFERENCES [operativo].[Tbl_Maest_Cliente] ([Id_Cliente])
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] CHECK CONSTRAINT [FK_Cliente_Contacto]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Contacto_Cliente_EstadoLOPDP] FOREIGN KEY([Id_Estado_LOPDP])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] CHECK CONSTRAINT [FK_Contacto_Cliente_EstadoLOPDP]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Contacto_Cliente_EstadoVerificado] FOREIGN KEY([Id_Estado_Verificacion])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] CHECK CONSTRAINT [FK_Contacto_Cliente_EstadoVerificado]
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Contacto_Cliente_TipoMedioContacto] FOREIGN KEY([Id_Tipo_Contacto])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Contacto_Cliente] CHECK CONSTRAINT [FK_Contacto_Cliente_TipoMedioContacto]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Direccion] FOREIGN KEY([Id_Cliente])
REFERENCES [operativo].[Tbl_Maest_Cliente] ([Id_Cliente])
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] CHECK CONSTRAINT [FK_Cliente_Direccion]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Direccion_Cliente_Estado_verificacion] FOREIGN KEY([Estado_Verificacion])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] CHECK CONSTRAINT [FK_Direccion_Cliente_Estado_verificacion]
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Direccion_Cliente_Tipo_Direccion] FOREIGN KEY([Id_Tipo_Direccion])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Direccion_Cliente] CHECK CONSTRAINT [FK_Direccion_Cliente_Tipo_Direccion]
GO
ALTER TABLE [operativo].[Tbl_Financiero_cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Financiero] FOREIGN KEY([Id_Cliente])
REFERENCES [operativo].[Tbl_Maest_Cliente] ([Id_Cliente])
GO
ALTER TABLE [operativo].[Tbl_Financiero_cliente] CHECK CONSTRAINT [FK_Cliente_Financiero]
GO
ALTER TABLE [operativo].[Tbl_Maest_Cliente]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_TipoIdentificacion] FOREIGN KEY([Id_Tipo_Identificacion])
REFERENCES [parametro].[Tbl_Cat_Item] ([Id_Item_Catalogo])
GO
ALTER TABLE [operativo].[Tbl_Maest_Cliente] CHECK CONSTRAINT [FK_Cliente_TipoIdentificacion]
GO
ALTER TABLE [operativo].[Tbl_Oficializacion_Core]  WITH CHECK ADD  CONSTRAINT [FK_Cliente_Oficializacion] FOREIGN KEY([Id_Cliente])
REFERENCES [operativo].[Tbl_Maest_Cliente] ([Id_Cliente])
GO
ALTER TABLE [operativo].[Tbl_Oficializacion_Core] CHECK CONSTRAINT [FK_Cliente_Oficializacion]
GO
ALTER TABLE [parametro].[Tbl_Cat_Item]  WITH CHECK ADD  CONSTRAINT [FK_Grupo_Item] FOREIGN KEY([Id_Grupo_Catalogo])
REFERENCES [parametro].[Tbl_Cat_Grupo] ([Id_Grupo_Catalogo])
GO
ALTER TABLE [parametro].[Tbl_Cat_Item] CHECK CONSTRAINT [FK_Grupo_Item]
GO
USE [master]
GO
ALTER DATABASE [DB_ODS] SET  READ_WRITE 
GO
