using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Category
{
    public int category_id ;
    public string category_name ;
}

[Serializable]
public class CategoryData
{
    public int status ;
    public List<Category> items ;
}