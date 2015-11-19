using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DarlFormDemo.Models
{
    public class Subs
    {
       
        [Display(Name = "Subscription key from your profile")]
        public Guid SubscriptionKey { get; set; }

        [Display(Name = "Map ID from the projects listing")]
        public Guid MapId { get; set; }
    }
}