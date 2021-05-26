
using Larpx.ResourceSpider.BaseLibrary.CharsetDetector.Core;
using System;

namespace Larpx.ResourceSpider.BaseLibrary.CharsetDetector.Prober
{
    
    /// <summary>
    /// This prober doesn't actually recognize a language or a charset.
    /// It is a helper prober for the use of the Hebrew model probers
    /// </summary>
    public class HebrewProber : CharsetProber
    {
        // windows-1255 / ISO-8859-8 code points of interest
        private const byte FINAL_KAF  = 0xEA;
        private const byte NORMAL_KAF = 0xEB;
        private const byte FINAL_MEM  = 0xED;
        private const byte NORMAL_MEM = 0xEE;
        private const byte FINAL_NUN  = 0xEF;
        private const byte NORMAL_NUN = 0xF0;
        private const byte FINAL_PE = 0xF3;
        private const byte NORMAL_PE = 0xF4;
        private const byte FINAL_TSADI = 0xF5;
        private const byte NORMAL_TSADI = 0xF6;

        // Minimum Visual vs Logical final letter score difference.
        // If the difference is below this, don't rely solely on the final letter score distance.
        private const int MIN_FINAL_CHAR_DISTANCE = 5;

        // Minimum Visual vs Logical model score difference.
        // If the difference is below this, don't rely at all on the model score distance.
        private const float MIN_MODEL_DISTANCE = 0.01f;

        protected const string VISUAL_HEBREW_NAME = "ISO-8859-8";
        protected const string LOGICAL_HEBREW_NAME = "windows-1255";
        
        // owned by the group prober.
        protected CharsetProber logicalProber, visualProber;
        protected int finalCharLogicalScore, finalCharVisualScore;      
        
        // The two last bytes seen in the previous buffer.
        protected byte prev, beforePrev;
                
        public HebrewProber()
        {
            Reset();
        }
         
        public void SetModelProbers(CharsetProber logical, CharsetProber visual) 
        { 
            logicalProber = logical; 
            visualProber = visual; 
        }
        
        /** 
         * Final letter analysis for logical-visual decision.
         * Look for evidence that the received buffer is either logical Hebrew or 
         * visual Hebrew.
         * The following cases are checked:
         * 1) A word longer than 1 letter, ending with a final letter. This is an 
         *    indication that the text is laid out "naturally" since the final letter 
         *    really appears at the end. +1 for logical score.
         * 2) A word longer than 1 letter, ending with a Non-Final letter. In normal
         *    Hebrew, words ending with Kaf, Mem, Nun, Pe or Tsadi, should not end with
         *    the Non-Final form of that letter. Exceptions to this rule are mentioned
         *    above in isNonFinal(). This is an indication that the text is laid out
         *    backwards. +1 for visual score
         * 3) A word longer than 1 letter, starting with a final letter. Final letters 
         *    should not appear at the beginning of a word. This is an indication that 
         *    the text is laid out backwards. +1 for visual score.
         *
         * The visual score and logical score are accumulated throughout the text and 
         * are finally checked against each other in GetCharSetName().
         * No checking for final letters in the middle of words is done since that case
         * is not an indication for either Logical or Visual text.
         *
         * The input buffer should not contain any white spaces that are not (' ')
         * or any low-ascii punctuation marks. 
         */
        public override ProbingState HandleData(byte[] buf, int offset, int len)
        {
            // Both model probers say it's not them. No reason to continue.
            if (GetState() == ProbingState.NotMe)
                return ProbingState.NotMe;

            int max = offset + len;

            for (int i = offset; i < max; i++) {
                
                byte b = buf[i];
                
                // a word just ended
                if (b == 0x20) {
                    // *(curPtr-2) was not a space so prev is not a 1 letter word
                    if (beforePrev != 0x20) {
                        // case (1) [-2:not space][-1:final letter][cur:space]
                        if (IsFinal(prev)) 
                            finalCharLogicalScore++;
                        // case (2) [-2:not space][-1:Non-Final letter][cur:space]                        
                        else if (IsNonFinal(prev))
                            finalCharVisualScore++;
                    }
                    
                } else {
                    // case (3) [-2:space][-1:final letter][cur:not space]
                    if ((beforePrev == 0x20) && (IsFinal(prev)) && (b != ' ')) 
                        ++finalCharVisualScore;
                }
                beforePrev = prev;
                prev = b;
            }

            // Forever detecting, till the end or until both model probers 
            // return NotMe (handled above).
            return ProbingState.Detecting;
        }

        // Make the decision: is it Logical or Visual?
        public override string GetCharsetName()
        {
            // If the final letter score distance is dominant enough, rely on it.
            int finalsub = finalCharLogicalScore - finalCharVisualScore;
            if (finalsub >= MIN_FINAL_CHAR_DISTANCE) 
                return LOGICAL_HEBREW_NAME;
            if (finalsub <= -(MIN_FINAL_CHAR_DISTANCE))
                return VISUAL_HEBREW_NAME;

            // It's not dominant enough, try to rely on the model scores instead.
            float modelsub = logicalProber.GetConfidence() - visualProber.GetConfidence();
            if (modelsub > MIN_MODEL_DISTANCE)
                return LOGICAL_HEBREW_NAME;
            if (modelsub < -(MIN_MODEL_DISTANCE))
                return VISUAL_HEBREW_NAME;
            
            // Still no good, back to final letter distance, maybe it'll save the day.
            if (finalsub < 0) 
                return VISUAL_HEBREW_NAME;

            // (finalsub > 0 - Logical) or (don't know what to do) default to Logical.
            return LOGICAL_HEBREW_NAME;
        }

        public override void Reset()
        {
            finalCharLogicalScore = 0;
            finalCharVisualScore = 0;
            prev = 0x20;
            beforePrev = 0x20;
        }

        public override ProbingState GetState() 
        {
            // Remain active as long as any of the model probers are active.
            if (logicalProber.GetState() == ProbingState.NotMe && 
                visualProber.GetState() == ProbingState.NotMe)
                return ProbingState.NotMe;
            return ProbingState.Detecting;
        }

        public override void DumpStatus()
        {
            Console.WriteLine("  HEB: {0} - {1} [Logical-Visual score]", 
               finalCharLogicalScore, finalCharVisualScore);
        }
        
        public override float GetConfidence()
        { 
            return 0.0f;
        }
        
        protected static bool IsFinal(byte b)
        {
            return (b == FINAL_KAF || b == FINAL_MEM || b == FINAL_NUN 
                    || b == FINAL_PE || b == FINAL_TSADI);        
        }
        
        protected static bool IsNonFinal(byte b)
        {
            // The normal Tsadi is not a good Non-Final letter due to words like 
            // 'lechotet' (to chat) containing an apostrophe after the tsadi. This 
            // apostrophe is converted to a space in FilterWithoutEnglishLetters causing 
            // the Non-Final tsadi to appear at an end of a word even though this is not 
            // the case in the original text.
            // The letters Pe and Kaf rarely display a related behavior of not being a 
            // good Non-Final letter. Words like 'Pop', 'Winamp' and 'Mubarak' for 
            // example legally end with a Non-Final Pe or Kaf. However, the benefit of 
            // these letters as Non-Final letters outweighs the damage since these words 
            // are quite rare.            
            return (b == NORMAL_KAF || b == NORMAL_MEM || b == NORMAL_NUN 
                    || b == NORMAL_PE);
        }
    }
}
