namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Representa una magnitud sinusoidal (tensión o corriente) en forma fasorial: un
/// vector con magnitud (valor RMS) y ángulo de fase en grados respecto a una
/// referencia común del sistema. Es la herramienta para razonar sobre desfase entre
/// ondas senoidales sin arrastrar la variable tiempo — el fasor asume **régimen
/// permanente senoidal puro** (una sola frecuencia fundamental).
/// </summary>
/// <remarks>
/// <b>Lo que este tipo NO modela: armónicos.</b> Una onda distorsionada (no senoidal
/// pura) se descompone en una serie de Fourier — fundamental + armónicos de orden
/// superior — y cada armónico es técnicamente su propio fasor a su propia frecuencia;
/// no se pueden sumar fasores de distinta frecuencia como si fueran uno solo. Ese
/// análisis (THD, espectro armónico) queda **fuera de v1**: no hay cargas no
/// lineales con requisitos de armónicos en el alcance actual, y modelarlo bien
/// (series de Fourier, THD, factor K de transformadores) es un motor aparte. Esta
/// nota queda aquí a propósito para que quien retome el proyecto sepa que la omisión
/// es consciente, no un descuido.
/// </remarks>
public readonly record struct Fasor
{
    public decimal Magnitud { get; }
    public decimal AnguloGrados { get; }

    public Fasor(decimal magnitud, decimal anguloGrados)
    {
        if (magnitud < 0)
            throw new ArgumentOutOfRangeException(nameof(magnitud), "La magnitud de un fasor (valor RMS) no puede ser negativa; un cambio de signo se expresa como 180° de ángulo, no como magnitud negativa.");

        Magnitud = magnitud;
        AnguloGrados = anguloGrados;
    }

    /// <summary>Componente real (en fase con la referencia, 0°).</summary>
    public decimal Real => (decimal)((double)Magnitud * Math.Cos(AGrados(AnguloGrados)));

    /// <summary>Componente imaginaria (en cuadratura, 90° respecto a la referencia).</summary>
    public decimal Imaginario => (decimal)((double)Magnitud * Math.Sin(AGrados(AnguloGrados)));

    public static Fasor DesdeRectangular(decimal real, decimal imaginario)
    {
        var magnitud = (decimal)Math.Sqrt((double)(real * real + imaginario * imaginario));
        var anguloGrados = (decimal)(Math.Atan2((double)imaginario, (double)real) * 180.0 / Math.PI);
        return new Fasor(magnitud, anguloGrados);
    }

    /// <summary>Suma fasorial (KCL/KVL): pasa por rectangular porque solo ahí sumar componentes tiene sentido físico.</summary>
    public static Fasor operator +(Fasor a, Fasor b) => DesdeRectangular(a.Real + b.Real, a.Imaginario + b.Imaginario);

    public static Fasor operator -(Fasor a, Fasor b) => DesdeRectangular(a.Real - b.Real, a.Imaginario - b.Imaginario);

    public static Fasor operator *(Fasor a, decimal escalar) =>
        escalar >= 0
            ? new Fasor(a.Magnitud * escalar, a.AnguloGrados)
            : new Fasor(a.Magnitud * -escalar, a.AnguloGrados + 180m);

    private static double AGrados(decimal grados) => (double)grados * Math.PI / 180.0;
}
