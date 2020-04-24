/// <reference path = "./base.page.ts" />
'use strict';
//鼠标拖拽	<div body="true" floatcenter="true" id="YYY" style="display:none"><div mousemove="{MoveId:'YYY'}" id="XXX"></div></div>
var AutoCSer;
(function (AutoCSer) {
    var MouseMove = (function () {
        function MouseMove(Parameter) {
            Pub.GetParameter(this, MouseMove.DefaultParameter, Parameter);
            this.MoveEvent = Pub.ThisEvent(this, this.Move);
            this.StopEvent = Pub.ThisFunction(this, this.Stop);
            this.Start(this.Event || DeclareEvent.Default);
        }
        MouseMove.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                if (this.IsStart)
                    this.Stop();
                var Element = HtmlElement.$IdElement(this.Id);
                if (Element != this.Element) {
                    this.Element = Element;
                    HtmlElement.$(document.body).AddEvent('mousedown,mouseup,blur', this.StopEvent).AddEvent('mousemove', this.MoveEvent);
                }
                var Element = HtmlElement.$IdElement(this.MoveId);
                Element.style.position = 'absolute';
                this.Left = Element.offsetLeft;
                this.Top = Element.offsetTop;
                this.ClientX = Event.clientX;
                this.ClientY = Event.clientY;
                this.IsStart = true;
            }
        };
        MouseMove.prototype.Move = function (Event) {
            HtmlElement.$Id(this.MoveId).Left(this.Left + Event.clientX - this.ClientX).Top(this.Top + Event.clientY - this.ClientY);
        };
        MouseMove.prototype.Stop = function () {
            if (this.IsStart) {
                HtmlElement.$(document.body).DeleteEvent('mousedown,mouseup,blur', this.StopEvent).DeleteEvent('mousemove', this.MoveEvent);
                this.IsStart = false;
            }
        };
        MouseMove.DefaultParameter = { Id: null, Event: null, MoveId: null };
        return MouseMove;
    }());
    MouseMove = MouseMove;
    new Declare(MouseMove, 'MouseMove', 'mousedown', 'AttributeName');
})(AutoCSer || (AutoCSer = {}));
