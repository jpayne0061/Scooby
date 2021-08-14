using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scooby.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class ValuesController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public ValuesController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(string obj)
        {
            if(obj == null)
            {
                return new string[0];
            }

            //referer is the url of the client website
            string referer = HttpContext.Request.Headers["Referer"];

            Dictionary<int, List<string>> refererObjects = null;

            _memoryCache.TryGetValue(referer, out refererObjects);

            if (refererObjects == null)
            {
                return new string[0];
            }

            int objectKey = GetObjectKey(JObject.Parse(obj));

            if (!refererObjects.ContainsKey(objectKey))
            {
                return new string[0];
            }

            return refererObjects[objectKey];
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] JObject value)
        {
            string referer = HttpContext.Request.Headers["Referer"];

            Dictionary<int, List<string>> refererObjects = null;

            _memoryCache.TryGetValue(referer, out refererObjects);

            if(refererObjects == null)
            {
                refererObjects = new Dictionary<int, List<string>>();

                _memoryCache.Set(referer, refererObjects);
            }

            if(GetDataSizeInBytes(refererObjects) > 250000)
            {
                return;
            }

            int objectKey = GetObjectKey(value);

            if (!refererObjects.ContainsKey(objectKey))
            {
                refererObjects[objectKey] = new List<string>();
            }

            refererObjects[objectKey].Add(value.ToString(Formatting.None));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private int GetObjectKey(JObject value)
        {
            List<string> objectProperties = new List<string>();

            foreach (JProperty prop in value.Properties())
            {
                objectProperties.Add(prop.Name);
            }

            int objectKey = string.Join(',', objectProperties.OrderBy(x => x)).GetHashCode();

            return objectKey;
        }

        private int GetDataSizeInBytes(Dictionary<int, List<string>> objects)
        {
            int size = 0;

            foreach (var kvp in objects)
            {
                size += kvp.Value.Sum(x => x.Length);
            }

            return size * 2;
        }
    }
}
