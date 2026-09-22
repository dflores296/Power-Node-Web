# El catálogo Square D no viaja a este repo

**CONFIRMADA · David · 2026-09-22** (propuesta originalmente por Claude, por razón legal; David la
confirmó y la amplió: el alcance de v1 tampoco lo necesita)

## La decisión

Nada de `Domain/Catalogo/`, `Data/Catalogo/` ni el PDF `COMPENDIADO 34 Schneider 2016 Web.zip` viaja
a `Power-Node-Web`, que es público.

## Por qué

Dos razones independientes, cualquiera bastaría sola:

1. **Legal.** El zip es una copia íntegra del catálogo de Schneider Electric, con derechos de autor
   de un tercero — redistribuirlo en un repo público es distinto de tenerlo en uno privado. Los
   *seeders* (`CatalogoTablerosSeeder.cs` y similares, 3 575 líneas) transcriben miles de números de
   parte y especificaciones de ese catálogo; aunque las especificaciones técnicas en sí suelen
   considerarse hechos no protegibles, con una marca registrada real de por medio esto no es un
   juicio que un asistente de código deba resolver por su cuenta — es orientación de sentido común,
   no asesoría legal, y merece una opinión profesional antes de publicar 2 552 modelos con nombre
   Square D en abierto.
2. **De alcance.** `docs/estado/05-alcance.md` de `PowerNode-DesignSuite` ya tiene la regla escrita:
   *"el catálogo es un atajo de captura, el cálculo nunca depende de él"*. Si el cálculo no depende
   del catálogo, esta versión no lo necesita para ser útil — captura a mano.

## Lo que se sigue de aquí

Ningún modelo de equipo, ninguna referencia a Square D, ningún dato de dimensiones/capacidades de
producto entra a este repo. Un usuario de la web teclea calibre, protección, longitud — igual que
hace `PowerNode-DesignSuite` cuando no hay modelo de catálogo elegido.
