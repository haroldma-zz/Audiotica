using System.Collections.Generic;

namespace Audiotica.Web.Models.Vk
{
    public class VkRoot
    {
        public VkError Error { get; set; }
        public List<object> Response { get; set; }
    }
}