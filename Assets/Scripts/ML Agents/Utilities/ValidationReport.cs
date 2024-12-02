using System.Collections.Generic;

public class ValidationReport
{
    public List<string> Errors { get; set; } = new List<string>();

    public bool Valid => Errors.Count == 0;

    public ValidationReport() { }

    public ValidationReport(List<string> errors)
    {
        Errors = errors;
    }
}
