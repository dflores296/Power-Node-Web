# Power Node Web

Calculadora de cuadros de carga según la **NOM-001-SEDE-2012**.

Un tablero de hasta 42 espacios, con la geometría real: nones a la izquierda, pares a la derecha, y
la fase de cada circuito derivada de su espacio y sus polos. Por cada circuito resuelve la
protección, los conductores de fase, neutro y tierra, y la caída de tensión.

**Cada resultado cita el artículo de la norma del que sale.** No es un número suelto: la memoria
dice *«240-6(a) · Capacidad mínima 7.09 A → protección estándar 15 A»*, de modo que quien revisa
puede tomar la fórmula del artículo citado y llegar al mismo resultado.

Corre completo en el navegador. Sin instalar nada, sin cuenta, sin servidor.

## Lo que calcula

| | Artículos |
|---|---|
| Corriente de diseño y carga continua al 125 % | 210-19(a)(1), 210-20(a) |
| Protección, con los valores normalizados de la NOM | 240-6(a) |
| Conductor por capacidad, con corrección por temperatura y agrupamiento | 310-15(b)(16)/(17), (b)(2)(a), (b)(3)(a) |
| Temperatura de terminal y crédito de aislamiento | 110-14(c), 310-104(a) |
| Caída de tensión por impedancia completa, no por fórmula aproximada | Tabla 9 |
| Conductor de puesta a tierra, con ajuste proporcional | 250-122, 250-122(b) |

## De dónde salen los números

De [**NOM-001-SEDE-2012**](https://github.com/dflores296/NOM-001-SEDE-2012): la norma completa,
estructurada y **verificada celda por celda contra el PDF del DOF**. Cada tabla que esta calculadora
usa viaja con la fecha en que se cotejó, y hay integración continua que rompe el build si los datos
publicados aquí se despegan de los de allá.

> Los resultados no sustituyen el criterio del ingeniero responsable del proyecto.

## Desarrollo

```bash
dotnet run --project src/PowerNode.Web
```

Blazor WebAssembly sobre .NET 8. Documentación del proyecto en [`docs/LEEME.md`](docs/LEEME.md).
