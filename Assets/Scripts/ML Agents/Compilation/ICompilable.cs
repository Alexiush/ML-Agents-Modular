using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Expression
{
    public string Id;
    public string Body;
}

public interface ICompilable
{
    public Expression Compile(CompilationContext compilationContext);
}
