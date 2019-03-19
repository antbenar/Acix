USE Acix_db;

SELECT dbo.producto.codigo AS Codigo, CONCAT(dbo.producto.codigo, ' / ' , dbo.marca.nombre,' / ' , grado, ' / ', contenido , ' / ', unidad) AS description 
FROM dbo.producto 
JOIN dbo.marca ON dbo.producto.marca_cod = dbo.marca.codigo
WHERE dbo.producto.codigo IN (
		SELECT dbo.Equivalencias.codigo1 + dbo.Equivalencias.codigo2 - dbo.producto.codigo as equivalentes
		FROM dbo.producto 
		JOIN dbo.Equivalencias ON dbo.producto.codigo = dbo.Equivalencias.codigo1 or dbo.producto.codigo = dbo.Equivalencias.codigo2
		WHERE dbo.producto.codigo = 3);
