/// <reference path = "./base.page.ts" />
'use strict';
//鼠标覆盖处理	<div mouseover="{}" id="XXX"></div>
var AutoCSer;
(function (AutoCSer) {
    var MouseOver = (function () {
        function MouseOver(Parameter) {
            Pub.GetParameter(this, MouseOver.DefaultParameter, Parameter);
            Pub.GetEvents(this, MouseOver.DefaultEvents, Parameter);
            this.Start(this.Event || DeclareEvent.Default);
        }
        MouseOver.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$IdElement(this.Id);
                if (Element != this.Element) {
                    this.Element = Element;
                    HtmlElement.$AddEvent(Element, ['mouseout'], Pub.ThisEvent(this, this.Out));
                }
                this.OnOver.Function(Event, Element);
            }
        };
        MouseOver.prototype.Out = function (Event) {
            this.OnOut.Function(Event, HtmlElement.$IdElement(this.Id));
        };
        MouseOver.DefaultParameter = { Id: null, Event: null };
        MouseOver.DefaultEvents = { OnOver: null, OnOut: null };
        return MouseOver;
    }());
    MouseOver = MouseOver;
    new Declare(MouseOver, 'MouseOver', 'mouseover', 'AttributeName');
})(AutoCSer || (AutoCSer = {}));
