Use Acix_db;
/*select month and year*/
/*
SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
FROM dbo.historial
WHERE vigente = 1 AND DAY(dia_hora)= '01'
AND
MONTH(dia_hora) = '03'
AND
YEAR(dia_hora) = '2018';
*/

/*//current month*/

SELECT Sum(ganancia), SUM (cantidad)
FROM dbo.historial
WHERE vigente = 1 AND MONTH(dia_hora) = MONTH(dateadd(dd, -1, GetDate()))
AND
YEAR(dia_hora) = YEAR(dateadd(dd, -1, GetDate()));


/*current day
SELECT codigo AS 'Codigo en Historial', dia_hora AS 'Día y hora', descripcion_producto AS 'Descripción del producto', cantidad AS 'Cantidad vendida', ganancia AS 'Ganancia'
FROM dbo.historial
WHERE vigente = 1 AND CONVERT (DATE, dia_hora) = CONVERT(date, Getdate());
*/