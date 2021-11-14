using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;
using MVC.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.WebPages.Html;
using Dota2Api;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<HeroesModel>       _model;
        private readonly ApiHandler              _handler; 
        public HomeController(ILogger<HomeController> logger)
        {
            _logger  = logger;
            _handler = new ApiHandler("8B037528963D8F3609DC86B3F1F486E3");// не бейте за хардкод, он одноразовый
            _model   = new List<HeroesModel>();

             InitHeroesSync();
        }

        public IActionResult Index()
        {
            return View(_model);
        }

        public IActionResult HeroInfo(int HeroID)
        {
            return View(GetMatchHistory(HeroID));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        private void InitHeroesSync()
        {
          var Heroes = _handler.GetHeroes();
            
          Heroes.Wait();

          if (Heroes is not null)
          {
            foreach (var Hero in Heroes.Result.Heroes)
            {
              var Item  = new HeroesModel();
              Item.Id   = Hero.Id;
              Item.Name = Hero.Name;
              Item.ValveName = Hero.ValveName;
              Item.Thumbnail = "/assets/" + Item.Name + ".png";

              _model.Add(Item);
            }
          }
        }
        
        private List<MatchInfoModel> GetMatchHistory(int HeroId)
        {
            List<MatchInfoModel> Model = new List<MatchInfoModel>();

            var HistoryTask = _handler.GetMatchHistory(null, null, HeroId, Dota2Api.Enums.GameMode.AllPick, null, -1, null, "10");
            HistoryTask.Wait();

            var Thumb = _model.Find(item => item.Id == HeroId).Thumbnail;

            foreach (var Match in HistoryTask.Result.Matches)
            {
                var MatchTask = _handler.GetDetailedMatch(Match.MatchId.ToString());
                MatchTask.Wait();

                MatchInfoModel MatchInfo = new MatchInfoModel();
                MatchInfo.MatchID = Match.MatchId.ToString();
                MatchInfo.PositiveVotes = MatchTask.Result.PositiveVotes;
                MatchInfo.NegativeVotes = MatchTask.Result.NegativeVotes;
                MatchInfo.StartTime = Match.StartTime;
                MatchInfo.HeroThumbnail = Thumb;

                foreach (var Player in MatchTask.Result.Players)
                {
                    if (Player.HeroId == HeroId)
                    {
                        MatchInfo.KDA = String.Format("{0}/{1}/{2}", Player.Kills, Player.Deaths, Player.Assists);
                        MatchInfo.Result = (MatchTask.Result.WinningFaction == Player.Faction ? "Win" : "Lose").ToString();

                        break;
                    }
                }

                Model.Add(MatchInfo);
            }

            return Model;
        }
        private long ToUnixTimestamp(DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = System.Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }
    }
}
