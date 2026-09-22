namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Una entrada del rastro de citas de un cálculo: qué artículo o tabla sustenta un paso, y qué se
/// decidió. Es la ventaja del proyecto sobre el Excel — cada resultado sabe de dónde viene, así
/// que la memoria de cálculo se redacta sola y con referencia exacta.
/// </summary>
public sealed record Cita(string Referencia, string Descripcion);
