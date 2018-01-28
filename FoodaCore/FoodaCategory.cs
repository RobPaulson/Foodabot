using System.Collections.Generic;

namespace FoodaCore
{
    public class FoodaCategory
    {

        public string CategoryName { get; set; }
        public List<FoodaItem> Items { get; set; }
    }
}