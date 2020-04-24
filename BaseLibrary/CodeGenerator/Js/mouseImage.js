/// <reference path = "./base.page.ts" />
'use strict';
//鼠标覆盖图片修改	<img mouseimage="{OverSrc:'over.jpg',OutSrc:'out.jpg'}" id="XXX" />
var AutoCSer;
(function (AutoCSer) {
    var MouseImage = (function () {
        function MouseImage(Parameter) {
            Pub.GetParameter(this, MouseImage.DefaultParameter, Parameter);
            this.Start(this.Event || DeclareEvent.Default);
        }
        MouseImage.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$IdElement(this.Id);
                if (Element != this.Element) {
                    this.Element = Element;
                    HtmlElement.$AddEvent(Element, ['mouseout'], Pub.ThisEvent(this, this.Out));
                }
                Element.src = this.OverSrc;
            }
        };
        MouseImage.prototype.Out = function (Event) {
            HtmlElement.$IdElement(this.Id).src = this.OutSrc;
        };
        MouseImage.DefaultParameter = { Id: null, Event: null, OverSrc: null, OutSrc: null };
        return MouseImage;
    }());
    MouseImage = MouseImage;
    new Declare(MouseImage, 'MouseImage', 'mouseover', 'AttributeName');
})(AutoCSer || (AutoCSer = {}));
