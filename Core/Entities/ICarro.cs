namespace Core.Entities
{
    public interface ICarro : IEntity
    {
        string Fabricante { get; set; }
        string Modelo { get; set; }
        string Cor { get; set; }
    }
}