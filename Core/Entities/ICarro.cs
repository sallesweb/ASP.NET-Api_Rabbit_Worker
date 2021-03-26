namespace Core.Entities
{
    public interface ICarro
    {
        string Fabricante { get; set; }
        string Modelo { get; set; }
        string Cor { get; set; }
    }
}