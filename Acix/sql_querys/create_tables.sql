/*CREATE DATABASE Acix_db;*/
USE Acix_db;

CREATE TABLE producto
(
	codigo INT not null IDENTITY(1,1),
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

CREATE TABLE historial
(
	codigo INT not null IDENTITY(1,1),
	descripcion_producto varchar(150),
	dia_hora datetime,
	cantidad INT,
	ganancia decimal(8,2),
	vigente INT,
	CONSTRAINT historial_pk PRIMARY KEY (codigo),
); 