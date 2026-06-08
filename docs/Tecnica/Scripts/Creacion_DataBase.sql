
USE [master]
GO

/****** Objeto: Database [DB_ODS_DEV1] ******/
CREATE DATABASE [DB_ODS_DEV1]
 CONTAINMENT = NONE
 COLLATE SQL_Latin1_General_CP1_CI_AS
GO

ALTER DATABASE [DB_ODS_DEV1] SET COMPATIBILITY_LEVEL = 150
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    EXEC [DB_ODS_DEV1].[dbo].[sp_fulltext_database] @action = 'enable'
END
GO

ALTER DATABASE [DB_ODS_DEV1] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ARITHABORT OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DB_ODS_DEV1] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DB_ODS_DEV1] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DB_ODS_DEV1] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DB_ODS_DEV1] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [DB_ODS_DEV1] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET RECOVERY FULL 
GO
ALTER DATABASE [DB_ODS_DEV1] SET  MULTI_USER 
GO
ALTER DATABASE [DB_ODS_DEV1] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DB_ODS_DEV1] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DB_ODS_DEV1] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DB_ODS_DEV1] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DB_ODS_DEV1] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DB_ODS_DEV1] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [DB_ODS_DEV1] SET QUERY_STORE = OFF
GO

USE [DB_ODS_DEV1]
GO

/****** Objeto: Schemas ******/
CREATE SCHEMA [operativo]
GO
CREATE SCHEMA [parametro]
GO
CREATE SCHEMA [seguridad]
GO

/****** Objeto: Table [parametro].[Tbl_Cat_Grupo] ******/
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

/****** Objeto: Table [parametro].[Tbl_Cat_Item] ******/
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

/****** Objeto: View [parametro].[Vw_Cat_Detalle_General] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [parametro].[Vw_Cat_Detalle_General]
WITH SCHEMABINDING 
AS
SELECT 
    i.Id_Item_Catalogo,
    i.Id_Grupo_Catalogo,
    g.Nombre_Grupo,
    i.Codigo_Valor,
    i.Texto_Visual,
    i.Orden_Visual,
    i.Esta_Activo,
    g.Es_Sistema
FROM parametro.Tbl_Cat_Item i
INNER JOIN parametro.Tbl_Cat_Grupo g ON i.Id_Grupo_Catalogo = g.Id_Grupo_Catalogo;
GO

/****** Objeto: Table [dbo].[__EFMigrationsHistory] ******/
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

/****** Objeto: Table [operativo].[Tbl_Contacto_Cliente] ******/
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

/****** Objeto: Table [operativo].[Tbl_Direccion_Cliente] ******/
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

/****** Objeto: Table [operativo].[Tbl_Financiero_cliente] ******/
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

/****** Objeto: Table [operativo].[Tbl_Maest_Cliente] ******/
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

/****** Objeto: Table [operativo].[Tbl_Oficializacion_Core] ******/
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

/****** Objeto: Table [seguridad].[Tbl_Maest_Usuario] ******/
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

/****** Indices y Configuraciones Extra ******/
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
CREATE NONCLUSTERED INDEX [IX_Contacto_Cliente_Valor] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Valor_Contacto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ContactoCliente_EstadoVerificacion] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Estado_Verificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ContactoCliente_IdCliente_EstaEliminado] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Cliente] ASC,
	[Esta_Eliminado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_Contacto_Cliente_Id_Estado_LOPDP] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Estado_LOPDP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_Contacto_Cliente_Id_Tipo_Contacto] ON [operativo].[Tbl_Contacto_Cliente]
(
	[Id_Tipo_Contacto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ContactoCliente_IdCliente_EstaEliminado_Dir] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Cliente] ASC,
	[Esta_Eliminado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Direccion_Cliente_IdCliente] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Direccion_Cliente] ASC,
	[Es_Principal] ASC
)
INCLUDE([Direccion_Completa],[Ciudad],[Latitud],[Longitud]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_Direccion_Cliente_Estado_Verificacion] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Estado_Verificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_Direccion_Cliente_Id_Tipo_Direccion] ON [operativo].[Tbl_Direccion_Cliente]
(
	[Id_Tipo_Direccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_FinancieroCliente_IdCliente] ON [operativo].[Tbl_Financiero_cliente]
(
	[Id_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_FinancieroCliente_TipoContabilidad] ON [operativo].[Tbl_Financiero_cliente]
(
	[Tipo_Contabilidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cliente_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
(
	[Identificacion_Cliente] ASC
)
WHERE ([Esta_Eliminado]=(0) AND [Esta_Anonimizado]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_Cliente_Nombre] ON [operativo].[Tbl_Maest_Cliente]
(
	[Nombre_Completo] ASC
)
WHERE ([Esta_Eliminado]=(0) AND [Esta_Anonimizado]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_Maest_Cliente_Id_Tipo_Identificacion] ON [operativo].[Tbl_Maest_Cliente]
(
	[Id_Tipo_Identificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_OficializacionCore_FechaCreacion] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Fecha_Creacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_OficializacionCore_IdCliente] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Id_Cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_OficializacionCore_RespuestaCoreCodigo] ON [operativo].[Tbl_Oficializacion_Core]
(
	[Respuesta_Core_Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_GrupoCatalogo_NombreGrupo] ON [parametro].[Tbl_Cat_Grupo]
(
	[Nombre_Grupo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ItemCatalogo_Buscador] ON [parametro].[Tbl_Cat_Item]
(
	[Id_Grupo_Catalogo] ASC,
	[Esta_Activo] ASC
)
INCLUDE([Codigo_Valor],[Texto_Visual],[Orden_Visual]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ItemCatalogo_IdGrupoCatalogo_CodigoValor] ON [parametro].[Tbl_Cat_Item]
(
	[Id_Grupo_Catalogo] ASC,
	[Codigo_Valor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usuario_CodigoUsuario] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Codigo_Usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usuario_Email] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Usuario_EstaActivo] ON [seguridad].[Tbl_Maest_Usuario]
(
	[Esta_Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Restricciones y Llaves Foráneas ******/
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
ALTER DATABASE [DB_ODS_DEV1] SET READ_WRITE 
GO

