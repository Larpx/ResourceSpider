/// <reference path = "./base.page.ts" />
'use strict';
//TextArea高度自适应控件	<textarea autoheight="{LineHeight:20}" id="XXX"></textarea>
var AutoCSer;
(function (AutoCSer) {
    var AutoHeight = (function () {
        function AutoHeight(Parameter) {
            Pub.GetParameter(this, AutoHeight.DefaultParameter, Parameter);
            var Element = HtmlElement.$IdElement(this.Id);
            if (this.LineHeight == null)
                this.LineHeight = parseInt(0 + HtmlElement.$AttributeOrStyle(Element, 'lineHeight'));
            this.SetFunction = Pub.ThisFunction(this, this.SetHeight);
            this.Div = HtmlElement.$Create('div').Display(0).AddClass(Element.className).Style('position', 'absolute').To();
            this.Start(this.Event || DeclareEvent.Default);
        }
        AutoHeight.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$IdElement(this.Id);
                if (!this.Element || Element != this.Element) {
                    this.MinHeight = (this.Element = Element).scrollHeight;
                    HtmlElement.$AddEvent(Element, ['blur'], Pub.ThisFunction(this, this.Stop));
                    HtmlElement.$AddEvent(Element, ['keypress', 'keyup'], this.SetFunction);
                    var Width = HtmlElement.$Width(Element), Css = HtmlElement.$Css(Element);
                    if (!Pub.IsBorder) {
                        Width -= parseInt(0 + Css['border-left-width'], 10) + parseInt(0 + Css['border-right-width'], 10);
                        if (!Pub.IsPadding)
                            Width -= parseInt(0 + Css['padding-left'], 10) + parseInt(0 + Css['padding-right'], 10);
                    }
                    this.Div.Style('width', Width + 'px');
                    if (this.Interval)
                        clearTimeout(this.Interval);
                    this.IsStart = false;
                }
                if (!this.IsStart) {
                    if (!this.Interval)
                        this.Interval = setTimeout(this.SetFunction, this.Timeout);
                    this.IsStart = true;
                }
            }
        };
        AutoHeight.prototype.SetHeight = function () {
            if (this.IsStart) {
                this.Div.Display(1).Html(this.Element.value.ToHTML().replace(/\r\n/g, '<br />').replace(/[\r\n]/g, '<br />'));
                var Height = this.Div.ScrollHeight0() + this.LineHeight;
                if (this.MaxHeight && Height > this.MaxHeight)
                    Height = this.MaxHeight;
                HtmlElement.$SetStyle(this.Element, 'height', Math.max(Height, this.MinHeight) + 'px');
                this.Div.Display(1);
                if (this.Interval)
                    clearTimeout(this.Interval);
                this.Interval = setTimeout(this.SetFunction, this.Timeout);
            }
        };
        AutoHeight.prototype.Stop = function () {
            if (this.Interval) {
                clearTimeout(this.Interval);
                this.Interval = 0;
            }
            this.IsStart = false;
        };
        AutoHeight.DefaultParameter = { Id: null, Event: null, LineHeight: null, MaxHeight: 0, Timeout: 200 };
        return AutoHeight;
    }());
    AutoHeight = AutoHeight;
    new Declare(AutoHeight, 'AutoHeight', 'focus', 'Src');
})(AutoCSer || (AutoCSer = {}));
