/// <reference path = "./base.page.ts" />
'use strict';
var AutoCSer;
(function (AutoCSer) {
    var CrawlTitle = (function () {
        function CrawlTitle(Parameter) {
            Pub.GetParameter(this, CrawlTitle.DefaultParameter, Parameter);
            setTimeout(Pub.ThisFunction(this, this.Request), 1);
        }
        CrawlTitle.prototype.Request = function () {
            var Link = this.Link.Link, Index = Link.indexOf('#');
            if ((Index + 1) && Link.charAt(Index + 1) != '!')
                Link = Link.substring(0, Index);
            HttpRequest.Post(this.AjaxCallName, { link: Link }, Pub.ThisFunction(this, this.OnLink));
        };
        CrawlTitle.prototype.OnLink = function (Value) {
            this.Link.IsTitle = true;
            if (Value.__AJAXRETURN__) {
                this.Link.Title = Pub.DeleteElements.Html(Value.__AJAXRETURN__).Text0();
                for (var Index = this.Link.CallBack.length; Index; this.Link.CallBack[--Index](this.Link))
                    ;
            }
        };
        CrawlTitle.Get = function (Link, CallBack) {
            var Value = this.Titles[Link];
            if (!Value)
                this.Links.push(this.Titles[Link] = Value = { Link: Link, Title: Link, CallBack: [], IsTitle: false });
            if (CallBack && !Value.IsTitle && Value.CallBack.IndexOfValue(CallBack) < 0)
                Value.CallBack.push(CallBack);
            return Value;
        };
        CrawlTitle.Request = function (AjaxCallName) {
            for (var Index = this.Links.length; Index; new CrawlTitle({ Link: this.Links[--Index], AjaxCallName: AjaxCallName }))
                ;
            this.Links = [];
        };
        CrawlTitle.TryRequest = function (AjaxCallName) {
            if (this.Links.length)
                setTimeout(Pub.ThisFunction(this, this.Request, [AjaxCallName]), 0);
        };
        CrawlTitle.DefaultParameter = { Link: null, AjaxCallName: null };
        CrawlTitle.Titles = {};
        CrawlTitle.Links = [];
        return CrawlTitle;
    }());
    CrawlTitle = CrawlTitle;
})(AutoCSer || (AutoCSer = {}));
