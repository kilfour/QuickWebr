namespace ContactManager.Core;

public class Contact(string name)
{
    public int Id { get; set; }
    public string Name { get; set; } = name;
}
