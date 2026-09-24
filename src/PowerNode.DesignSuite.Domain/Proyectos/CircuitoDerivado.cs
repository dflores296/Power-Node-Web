using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

public class CircuitoDerivado
{
    public int Id { get; set; }
    public int TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;
    public TipoCarga TipoCarga { get; set; }

    /// <summary>
    /// Los renglones de carga del circuito (Alumbrado/Contactos) — el VA sale de sumarlos, no se
    /// captura agregado. Vacío en Fuerza (ver DatosMotor).
    /// </summary>
    public List<ElementoCircuito> Elementos { get; set; } = [];

    /// <summary>
    /// <b>Dónde va montado</b>: el espacio del tablero y el interruptor que lleva. Obligatorio — un
    /// circuito derivado siempre nace de un dispositivo de protección montado en algún lado, y por
    /// definición del Artículo 100 ese dispositivo es <b>el final</b>: de ahí a las salidas.
    ///
    /// <para>
    /// Hasta el 2026-08-20 esto eran cuatro campos de esta misma clase, y por eso solo un circuito
    /// derivado podía ocupar un espacio. Ver <see cref="EspacioTablero"/> para por qué eso era un
    /// hueco y no una simplificación.
    /// </para>
    /// </summary>
    public int EspacioTableroId { get; set; }

    /// <summary>
    /// El espacio donde va montado. <b>Nace con el circuito</b>, no se asigna después: un circuito
    /// derivado sin dispositivo de protección no es nada — es cable suelto. Arrancar con uno vacío
    /// (espacio 0, 1 polo) es lo mismo que hacía antes un circuito recién creado, cuando estos
    /// campos vivían aquí dentro.
    /// </summary>
    public EspacioTablero Espacio { get; set; } = new();

    // ------------------------------------------------------------------ atajos al montaje
    //
    // NO SON CAMPOS: leen y escriben los del espacio. El dato vive en UN solo lugar, y aquí se
    // expone con el nombre con el que se le habla a un circuito -- "el circuito 12 es de 2 polos" es
    // como se dice en obra, aunque el número y los polos sean del espacio donde va atornillado.
    //
    // Están marcados como no mapeados: para EF esta clase no tiene estas columnas, las tiene la de
    // espacios. Si alguien los quita, lo que sigue siendo verdad es que el dato está en el espacio.

    /// <inheritdoc cref="EspacioTablero.Numero"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Numero
    {
        get => Espacio.Numero;
        set => Espacio.Numero = value;
    }

    /// <inheritdoc cref="EspacioTablero.Polos"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Polos
    {
        get => Espacio.Polos;
        set => Espacio.Polos = value;
    }

    /// <inheritdoc cref="EspacioTablero.Fase"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Fase
    {
        get => Espacio.Fase;
        set => Espacio.Fase = value;
    }

    /// <inheritdoc cref="EspacioTablero.ModeloCatalogoProteccion"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ModeloCatalogoProteccion
    {
        get => Espacio.ModeloCatalogoProteccion;
        set => Espacio.ModeloCatalogoProteccion = value;
    }

    /// <inheritdoc cref="EspacioTablero.NeutroConmutado"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool NeutroConmutado
    {
        get => Espacio.NeutroConmutado;
        set => Espacio.NeutroConmutado = value;
    }

    /// <inheritdoc cref="EspacioTablero.EspacioOcupadoPlg"/>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal? EspacioOcupadoPlg
    {
        get => Espacio.EspacioOcupadoPlg;
        set => Espacio.EspacioOcupadoPlg = value;
    }


    public decimal LongitudM { get; set; }

    // EL FACTOR DE DEMANDA YA NO VIVE AQUÍ (2026-08-20). Estaba en el circuito derivado y lo
    // multiplicaba, o sea que encogía SU conductor y SU interruptor — y la norma lo prohíbe con todas
    // sus letras:
    //
    //   220-42: «Esos factores NO SE DEBEN APLICAR para calcular el número de circuitos derivados
    //            para iluminación general.»
    //   220-40: la carga del ALIMENTADOR es la suma de los derivados «después de aplicar cualquier
    //            factor de demanda aplicable».
    //
    // O sea: el factor es del alimentador y de la acometida, sobre la suma de todo lo que cuelga.
    // Aplicado aquí subdimensionaba el derivado, que es la dirección peligrosa. Vive ahora en
    // Alimentador.FactorDemandaContinua.


    /// <summary>
    /// Si la carga es no lineal (electrónica, VFD, LED, etc.), el neutro se cuenta como conductor
    /// portador de corriente para el factor de agrupamiento — 310-15(b)(5)(3).
    /// </summary>
    public bool CargaLineal { get; set; } = true;

    /// <summary>Número de conductores en paralelo por fase, elegido por el usuario (mínimo 1). Solo válido a partir de 1/0 AWG (310-10(h)(1)).</summary>
    public int NumeroConductoresParalelo { get; set; } = 1;

    public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;
    public MaterialCanalizacion MaterialCanalizacion { get; set; } = MaterialCanalizacion.Pvc;

    /// <summary>
    /// Designación del aislamiento del conductor (p.ej. "THHN", "THW", "TW") — Tabla 310-104(a).
    /// Debe alcanzar o superar la temperatura que exige la terminal del equipo (110-14(c)); si no,
    /// el cálculo truena con <see cref="Calculo.Casos.AislamientoIncompatibleException"/>.
    /// </summary>
    public string TipoAislamiento { get; set; } = "THHN";

    /// <summary>Lugar de instalación para efectos de la Tabla 310-104(a) -- true = seco, false = húmedo/mojado. Algunas designaciones (p.ej. THHW) tienen rating distinto según el lugar.</summary>
    public bool LugarInstalacionSeco { get; set; } = true;

    /// <summary>Canalización/cable (310-15(b)(16)) o al aire libre (310-15(b)(17)) -- ambas base 30°C.</summary>
    public MetodoInstalacion MetodoInstalacion { get; set; } = MetodoInstalacion.CanalizacionOCable;

    /// <summary>Temperatura ambiente del sitio de instalación — Tabla 310-15(b)(2)(a). Default 30°C (factor 1, sin corrección).</summary>
    public decimal TemperaturaAmbienteC { get; set; } = 30m;

    /// <summary>
    /// Conductores portadores de corriente que comparten canalización con este circuito (contando
    /// los de otros circuitos, no solo los de este) — Tabla 310-15(b)(3)(a). Default 3 (sin
    /// ajuste); no se deriva de un editor de gabinete todavía, es un dato capturado.
    /// </summary>
    public int ConductoresAgrupados { get; set; } = 3;

    /// <summary>Factor de potencia (cos θ) de la carga, para la caída de tensión.</summary>
    public decimal FactorPotencia { get; set; } = 0.9m;

    /// <summary>
    /// <b>Declaración del proyectista:</b> el ensamble —envolvente y dispositivos de sobrecorriente
    /// juntos— está aprobado para operar al <b>100 %</b> de su valor nominal, y por lo tanto no se le
    /// aplica el 125 % de la carga continua. <b>Falso por omisión</b>, que es la regla general.
    ///
    /// <para>
    /// <b>Es dato de placa y de marcado, no una opción de diseño</b>, y por eso se declara en vez de
    /// deducirse: la excepción de 210-19(a)(1) y 210-20(a) habla del <b>ensamble</b>, no del interruptor, y hay
    /// gabinetes marcados <i>"Not rated for 100% rated circuit breakers"</i>. El programa lo
    /// contradice cuando el modelo elegido del catálogo es de 80 % —ahí sí hay evidencia—, pero no lo
    /// concede solo. Ver <c>CargaContinua100Pct</c>.
    /// </para>
    /// </summary>
    public bool ConjuntoAprobado100Pct { get; set; }


    public CalculoCircuitoDerivado? Calculo { get; set; }

    /// <summary>Datos propios de un circuito de Fuerza (motor). Null salvo cuando TipoCarga = Fuerza.</summary>
    public DatosMotor? DatosMotor { get; set; }

    /// <summary>
    /// Reescribe el <see cref="ElementoCircuito.VaUnitario"/> de cada renglón que se haya capturado en
    /// watts o en amperes. <b>Hay que llamarla antes de sumar la carga</b>, y por eso la llaman los dos
    /// únicos lugares que la suman: <c>CalculoCircuitoDerivadoService</c> y <c>CascadaCalculoService</c>.
    ///
    /// <para>
    /// <b>Por qué no basta con convertir al capturar.</b> La conversión depende de la tensión del
    /// tablero y del factor de potencia del circuito, y las dos se pueden cambiar después de haber
    /// capturado el renglón. Un VA congelado en el momento de teclearlo quedaría desfasado sin que
    /// nada lo dijera — y ese número sale impreso en la memoria de cálculo. Normalizar aquí es
    /// barato, es idempotente, y hace que el dato guardado (lo que dice la placa) y el dato calculado
    /// no se puedan separar.
    /// </para>
    ///
    /// <para>
    /// <b>Los renglones capturados en VA no se tocan:</b> ahí <c>ValorConsumo</c> es <c>null</c> y
    /// manda <c>VaUnitario</c>, que es lo que hacían todos los renglones antes del 2026-08-21.
    /// </para>
    /// </summary>
    public void NormalizarConsumoDeRenglones()
    {
        foreach (var elemento in Elementos)
            if (elemento.ValorConsumo is not null)
                elemento.VaUnitario = VaUnitarioDe(elemento);
    }

    /// <summary>
    /// Los volt-amperes por unidad que le corresponden a un renglón, <b>sin escribir nada</b>.
    ///
    /// <para>
    /// La pantalla la usa para enseñar el VA en el momento de teclear la placa;
    /// <see cref="NormalizarConsumoDeRenglones"/> la usa para dejarlo escrito antes de calcular.
    /// <b>Es la misma cuenta y por eso es un solo método</b>: si la pantalla y el motor convirtieran
    /// cada uno por su lado, el VA que se ve y el que se calcula podrían no ser el mismo, y ése es
    /// exactamente el tipo de diferencia que nadie encuentra hasta que ya está impresa.
    /// </para>
    /// </summary>
    public decimal VaUnitarioDe(ElementoCircuito elemento)
    {
        if (elemento.ValorConsumo is not { } valor || Tablero is not { } tablero)
            return elemento.VaUnitario;

        var numeroFases = string.IsNullOrWhiteSpace(Fase) ? Math.Max(Polos, 1) : Fase.Length;

        // La misma regla que corre el motor, y en un solo lugar: SistemaDelTablero. Estaba escrita
        // como "Fases >= 2 ? /√3" en ocho sitios, y con esa forma un centro de carga de 240 V daba
        // 138.6 V de fase-neutro en vez de 120.
        var tensionFF = tablero.TensionV;
        var tensionFN = PowerNode.DesignSuite.Calculo.Tableros.SistemaDelTablero
            .TensionFaseNeutro(tensionFF, tablero.Sistema);

        // Namespace completo: esta clase tiene una propiedad llamada Calculo, y C# resuelve
        // "Calculo.Casos" contra ella antes que contra el espacio de nombres.
        return PowerNode.DesignSuite.Calculo.Casos.ConsumoDePlaca.AVoltAmperes(
            valor, elemento.UnidadConsumo, tensionFN, tensionFF, numeroFases, FactorPotencia);
    }
}
