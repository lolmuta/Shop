USE [TestDB]
GO
/****** Object:  Table [dbo].[BuyItems]    Script Date: 2024/3/5 上午 01:43:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BuyItems](
	[IDNo] [int] IDENTITY(1,1) NOT NULL,
	[buyCount] [int] NULL,
	[Items_IDNo] [int] NOT NULL,
	[UserId] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Items]    Script Date: 2024/3/5 上午 01:43:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Items](
	[IDNo] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NULL,
	[count] [int] NULL,
	[src] [nvarchar](50) NULL,
	[price] [decimal](18, 0) NULL
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[BuyItems] ON 

INSERT [dbo].[BuyItems] ([IDNo], [buyCount], [Items_IDNo], [UserId]) VALUES (1, 4, 1, N'1')
INSERT [dbo].[BuyItems] ([IDNo], [buyCount], [Items_IDNo], [UserId]) VALUES (2, 41, 2, N'1')
SET IDENTITY_INSERT [dbo].[BuyItems] OFF
GO
SET IDENTITY_INSERT [dbo].[Items] ON 

INSERT [dbo].[Items] ([IDNo], [name], [count], [src], [price]) VALUES (1, N'商品1', 21, N'img/camera.jpg', CAST(123 AS Decimal(18, 0)))
INSERT [dbo].[Items] ([IDNo], [name], [count], [src], [price]) VALUES (2, N'商品2', 12, N'img/camera.jpg', CAST(33 AS Decimal(18, 0)))
SET IDENTITY_INSERT [dbo].[Items] OFF
GO
