// ***********************************************************************
// Assembly         : DarlFormDemo
// Author           : Andrew
// Created          : 11-19-2015
//
// Last Modified By : Andrew
// Last Modified On : 11-19-2015
// ***********************************************************************
// <copyright file="HomeController.cs" company="Dr Andy's IP Ltd (BVI)">
//     Copyright ©  2015
// </copyright>
// <summary>Simple demo of ConceptForms in ASP.Net</summary>
// ***********************************************************************
using DarlCommon;
using DarlFormDemo.Properties;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DarlFormDemo.Models;

namespace DarlFormDemo.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Ask for the subscription key and Map ID
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Store the subs keys.
        /// </summary>
        /// <param name="sub">The sub.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Subs sub)
        {
            if (ModelState.IsValid)
            {
                Session["subs"] = sub; //store it in the session
                return RedirectToAction("DFRun");
            }
            return View();
        }


        /// <summary>
        /// Runs a DarlForms form.
        /// </summary>
        /// <returns>
        /// Task&lt;ActionResult&gt;.
        /// </returns>
        public async Task<ActionResult> DFRun()
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            var sub = Session["subs"] as Subs;
            queryString["subscription-key"] = sub.SubscriptionKey.ToString();
            var uri = Settings.Default.QSetPath + "/" + sub.MapId.ToString() + "?" + queryString;
            var response = await client.GetAsync(uri);

            if (response.Content != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    QuestionSetProxy qsp = JsonConvert.DeserializeObject<QuestionSetProxy>(responseString);
                    if (qsp.questions == null && qsp.responses == null) //bad response
                        return RedirectToAction("ContentNotFound");
                    return View(qsp);
                }
                else if ((int)response.StatusCode == 429)
                {
                    ModelState.AddModelError("", "Rate limit exceeded - wait and try again or upgrade to Unlimited");
                    return View();
                }
                else
                {
                    ViewBag.ErrorText = await response.Content.ReadAsStringAsync();
                    return View("Error");
                }
            }
            return RedirectToAction("ContentNotFound");
        }

        /// <summary>
        /// Runs a DarlForms form.
        /// </summary>
        /// <param name="backButton">The back button.</param>
        /// <param name="nextButton">The next button.</param>
        /// <param name="qsp">The updated QuestionSetProxy.</param>
        /// <returns>
        /// A new QuestionSetProxy containing the next questions or the results.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> DFRun(string backButton, string nextButton, QuestionSetProxy qsp)
        {
            if (ModelState.IsValid)
            {
                var client = new HttpClient();
                var sub = Session["subs"] as Subs;
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["subscription-key"] = sub.SubscriptionKey.ToString();
                if (!string.IsNullOrEmpty(nextButton))
                {
                    var uri = Settings.Default.QSetPath + "?" + queryString;

                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, qsp);

                    if (response.Content != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            QuestionSetProxy nqsp = JsonConvert.DeserializeObject<QuestionSetProxy>(responseString);
                            ModelState.Clear();
                            return View(nqsp);
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            ModelState.AddModelError("", "Rate limit exceeded - wait and try again or upgrade to Unlimited");
                            return View(qsp);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(backButton))
                {
                    var uri = Settings.Default.QSetPath + "/" + qsp.ieToken + "?" + queryString;
                    HttpResponseMessage response = await client.DeleteAsync(uri);
                    if (response.Content != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            QuestionSetProxy nqsp = JsonConvert.DeserializeObject<QuestionSetProxy>(responseString);
                            ModelState.Clear();
                            return View(nqsp);
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            ModelState.AddModelError("", "Rate limit exceeded - wait and try again or upgrade to Unlimited");
                            return View(qsp);
                        }
                    }
                }
            }
            return View();
        }
    }
}