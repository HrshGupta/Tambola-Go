using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SubCategory
{
    public int category_id ;
    public int sub_category_id ;
    public string sub_category_name ;
}

[Serializable]
public class SubCategoryData
{
    public int status ;
    public List<SubCategory> items ;
}