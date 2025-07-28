public class Voisin(string ip, string name)
{
    public string Ip { get; } = ip ?? throw new ArgumentNullException(nameof(ip));
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

}
