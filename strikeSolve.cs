using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class strikeSolve : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMBombModule Module;
   public KMBombInfo Info;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   public KMSelectable topBtn;
   public KMSelectable bottomBtn;

   public TextMesh TopText;
   public TextMesh BottomText;

   private int submitStep;
   private string submitAnswer = "";

   void Log(string text, params object[] args)
   {
      Debug.LogFormat("[SolveStrike #{0}] {1}", ModuleId, string.Format(text, args));
   }

   Color getRandomColor()
   {
      switch (Rnd.Range(0, 6))
      {
         case 0:
            return Color.white;
         case 1:
            return Color.red;
         case 2:
            return Color.blue;
         case 3:
            return Color.green;
         case 4:
            return Color.yellow;
         case 5:
            return Color.magenta;
         default:
            return Color.white;
            
      }
   }

   string ColorToWord(Color input)
   {
      if (input == Color.white) return "White";
      if (input == Color.red) return "Red";
      if (input == Color.blue) return "Blue";
      if (input == Color.green) return "Green";
      if (input == Color.yellow) return "Yellow";
      if (input == Color.magenta) return "Magenta";
      return "White";
   }

   int BetaGammaOffset(Color input)
   {
      if (input == Color.white) return 2;
      if (input == Color.red) return -1;
      if (input == Color.blue) return 3;
      if (input == Color.green) return -2;
      if (input == Color.yellow) return 1;
      if (input == Color.magenta) return -3;
      return 0;
   }

   char TBGet(char input)
   {
      if ("ADEGHLMPQRTVWY".Contains(input)) return 'T';
      return 'B';
   }
   
   void Awake () {
      

   }

   void TopPress()
   {
      if (submitAnswer == "") return;
      topBtn.AddInteractionPunch();
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, topBtn.transform);
      if (submitAnswer[submitStep] == 'T')
      {
         submitStep++;
         if (submitStep == 4)
         {
            Log("SolveStrike Solved");
            Module.HandlePass();
            
         }
      }
      else
      {
         submitStep = 0;
         Log("SolveStrike Struck! Input Reset");
         Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
         Module.HandleStrike();
      }

   }
   void BottomPress()
   {
      if (submitAnswer == "") return;
      bottomBtn.AddInteractionPunch();
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, bottomBtn.transform);
      if (submitAnswer[submitStep] == 'B')
      {
         submitStep++;
         if (submitStep == 4)
         {
            Log("SolveStrike Solved");
            Module.HandlePass();
         }
      }
      else
      {
         submitStep = 0;
         Log("SolveStrike Strike");
         Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
         Module.HandleStrike();
      }
   }
   

   void Start () {
      ModuleId = ModuleIdCounter++;
      
      
      
      topBtn.OnInteract += delegate () { TopPress(); return false; };
      bottomBtn.OnInteract += delegate () { BottomPress(); return false; };

      Log("SolveStrike Generating");
      Color TopBtnColor = getRandomColor();
      Color BottomBtnColor = getRandomColor();

      String[] solveVarients = new[] { "SOLVE", "SALVE", "SONVE", "SELVE", "SONKE", "SILVE", "SAMVE"};
      String[] strikeVarients = new[] { "STRIKE", "STONKE", "STRUCK", "STROKE", "STUNKE", "STOMKE", "STAMKE" };
      String solve = solveVarients[Rnd.Range(0, solveVarients.Length)];
      String strike = strikeVarients[Rnd.Range(0, strikeVarients.Length)];
      
      TopText.color = TopBtnColor;
      BottomText.color = BottomBtnColor;
      TopText.text = solve;
      BottomText.text = strike;
      
      Log("Top Button {0} ({1})", solve, ColorToWord(TopBtnColor));
      Log("Bottom Button {0} ({1})", strike, ColorToWord(BottomBtnColor));

      int alpha_offset = 0;
      if (Bomb.GetBatteryCount() >= 3) alpha_offset += 2;
      if (Bomb.GetPorts().Contains("Parallel")) alpha_offset += 1;
      if (Bomb.GetSerialNumberNumbers().Last() % 2 == 1) alpha_offset -= 3;
      bool isLetter = false;
      foreach (var c in Bomb.GetSerialNumber())
      {
         if (solve.Contains(c) || strike.Contains(c))
         {
            isLetter = true;
         }
      }
      if (isLetter) alpha_offset += 3;
      if (Bomb.GetOnIndicators().Count() <= 2) alpha_offset -= 2;

      if (Bomb.GetModuleIDs().Any(x => x == "MemoryV2" || x == "organizationModule" || x == "SouvenirModule"))
      {
         Log("Souvenir, Memory, or Organization detected");
         alpha_offset = 0;
      }
      //top color is green blue or yellow
      if (TopBtnColor == Color.green || TopBtnColor == Color.blue || TopBtnColor == Color.yellow) alpha_offset += 2;
      if (TopBtnColor == Color.red || TopBtnColor == Color.magenta || TopBtnColor == Color.white) alpha_offset -= 1;
      if (Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count()) alpha_offset += 1;
      if (Bomb.GetModuleIDs().Contains("strikeSolve")) alpha_offset = 7;

      int betaOffset = BetaGammaOffset(TopBtnColor);
      int gammaOffset = BetaGammaOffset(BottomBtnColor);
      Log("Alpha Offset: {0}", alpha_offset);
      Log("Beta Offset: {0}", betaOffset);
      Log("Gamma Offset: {0}", gammaOffset);

      string AlphabetKey = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      //first, remove any duplicates from solve
      string solveKey = "";
      foreach (var c in solve)
      {
         if (!solveKey.Contains(c))
         {
            solveKey += c;
         }
      }
      //then, remove any duplicates from strike, and if they also appear in solveKey, remove them
      string strikeKey = "";
      foreach (var c in strike)
      {
         if (!strikeKey.Contains(c) && !solveKey.Contains(c))
         {
            strikeKey += c;
         }
      }
      string combinedKey = solveKey.ToUpper() + strikeKey.ToUpper();
      foreach (var c in AlphabetKey)
      {
         if (combinedKey.Contains(c)) AlphabetKey = AlphabetKey.Replace(c.ToString(), "");
      }
      string finalKey = solveKey.ToUpper() + strikeKey.ToUpper() + AlphabetKey;
      //remove the last character
      finalKey = finalKey.Substring(0, finalKey.Length - 1);
      Log("Key: {0}", finalKey);
      int deltaOffset = (((alpha_offset + betaOffset + gammaOffset + 1) * (Bomb.GetBatteryHolderCount() + 1)) % 24) + 1;
      Log("Delta Offset: {0}", deltaOffset);
      int EpsilonOffset = deltaOffset + Math.Abs(betaOffset);
      Log("Epsilon Offset: {0}", EpsilonOffset);
      int ZetaOffset = EpsilonOffset + Math.Abs(gammaOffset);
      Log("Zeta Offset: {0}", ZetaOffset);
      int EtaOffset = deltaOffset - Math.Abs(betaOffset);
      Log("Eta Offset: {0}", EtaOffset);
      int ThetaOffset = EtaOffset - Math.Abs(gammaOffset);
      Log("Theta Offset: {0}", ThetaOffset);

      string debugKey = finalKey + finalKey + finalKey;
      //we going to consider index 25 as "0", and be moving back and forth using our offsets
      int index = 25;
      char EpsilonChar = debugKey[index + EpsilonOffset - 1];
      Log("Epsilon Char: {0}", EpsilonChar);
      char ZetaChar = debugKey[index + ZetaOffset - 1];
      Log("Zeta Char: {0}", ZetaChar);
      char EtaChar = debugKey[index + EtaOffset - 1];
      Log("Eta Char: {0}", EtaChar);
      char ThetaChar = debugKey[index + ThetaOffset - 1];
      Log("Theta Char: {0}", ThetaChar);

      string TB = "";
      TB += TBGet(EpsilonChar);
      TB += TBGet(ZetaChar);
      TB += TBGet(EtaChar);
      TB += TBGet(ThetaChar);
      
      Log("Submit: {0}", TB);
      submitAnswer = TB;
   }

   void Update () {

   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} T or B or a sequence such as TTBB to press the top or bottom button.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Log("Processing Twitch Command - {0}", Command);
      Command = Command.Trim().ToUpper();
      if (submitAnswer == "")
      {
         yield return "sendtochaterror The module has not generated yet, this may be a bug.";
      }
      foreach (char c in Command)
      {
         if (c == 'T')
         {
            yield return null;
            TopPress();
         }
         else if (c == 'B')
         {
            yield return null;
            BottomPress();
         }
         else
         {
            yield return "sendtochaterror Invalid command.";
            yield break;
         }
         
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
