/*CREATE DATABASE Acix_db;*/
USE Acix_db;
/*
CREATE TABLE proveedor
(
	codigo INT not null IDENTITY(1,1),
	nombre varchar(50),
	direccion varchar(150),
	telefono varchar(20),
	CONSTRAINT proveedor_pk PRIMARY KEY (codigo)
); 

CREATE TABLE producto
(
	codigo INT not null IDENTITY(1,1),
	proveedor_nombre varchar(50),
	marca varchar(30),
	grado varchar(30),
	contenido varchar(30),
	unidad varchar(30),
	stock INT,
	precio_venta decimal(8,2),
	precio_compra decimal(8,2),
	CONSTRAINT producto_pk PRIMARY KEY (codigo),
); 

CREATE TABLE Equivalencias
(
	codigo1 INT,
	codigo2 INT,
	CONSTRAINT producto1_pk PRIMARY KEY (codigo1,codigo2),
	CONSTRAINT Equivalencias_fk_producto1 FOREIGN KEY (codigo1) REFERENCES producto(codigo),
	CONSTRAINT Equivalencias_fk_producto2 FOREIGN KEY (codigo2) REFERENCES producto(codigo)
);

CREATE TABLE cliente
(
	codigo INT not null IDENTITY(1,1),
	nombre varchar(50),
	apellidos varchar(50),
	telefono varchar(50),
	marca varchar(50),
	vehiculo varchar(50),
	motor varchar(50),
	tipo_aceite varchar(50),
	tipo_filtro varchar(50),
	CONSTRAINT cliente_pk PRIMARY KEY (codigo)
); 

CREATE TABLE historial
(
	codigo INT not null IDENTITY(1,1),
	cliente_codigo INT not null,
	nombres_cliente varchar(150),
	descripcion_producto varchar(150),
	dia_hora datetime,
	cantidad INT,
	ganancia decimal(8,2),
	precio_venta decimal(8,2),
	vigente INT,
	CONSTRAINT historial_pk PRIMARY KEY (codigo),
); 

CREATE TABLE comprobante
(
	codigo INT not null IDENTITY(1,1),
	nombre varchar(50),
	serie varchar(4),
	numero INT,
	cod_historial INT,
	CONSTRAINT comprobante_pk PRIMARY KEY (codigo),
); 

INSERT INTO dbo.comprobante (nombre, serie, numero, cod_historial) VALUES ('Factura','004-',00748,'1');
INSERT INTO dbo.comprobante (nombre, serie, numero) VALUES ('Boleta','004-',00749);
INSERT INTO dbo.comprobante (nombre, serie, numero) VALUES ('Nota de Pedido','004-',00750);


CREATE TABLE gastos
(
	codigo INT not null IDENTITY(1,1),
	descripcion varchar(80),
	costo decimal(8,2),
	dia_hora datetime,
	CONSTRAINT gastos_pk PRIMARY KEY (codigo),
); 
*/
CREATE TABLE caja_chica
(
	codigo INT not null IDENTITY(1,1),
	monto_inicial decimal(8,2),
	monto_actual decimal(8,2),
	gastos decimal(8,2),
	dia_hora datetime,
	CONSTRAINT caja_chica_pk PRIMARY KEY (codigo),
); 