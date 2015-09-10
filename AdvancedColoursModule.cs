using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExpandedColourOptions
{
    [KSPModule("Advanced Colour Options")]
    public class AdvancedColoursModule : PartModule
    {

        public string rByte, gByte, bByte;
        public string hexColour;
        public bool hexInput = false;
        public bool guiOpen = false;
        public bool applyToSymmetry = true;
        public Rect _winPos = new Rect();
        public Texture previewTexture = new Texture();
        public bool hasSetupStyles = false;
        public GUIStyle 
            _winStyle,
            _buttonStyle,
            _labelStyle,
            _toggleStyle,
            _textFieldStyle,
            _labelStyleRichText;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Advanced Colour Options")]
        public void showGUI()
        {

            if (guiOpen)
            {
                RenderingManager.RemoveFromPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
                guiOpen = false;
                previewTexture = new Texture(); // Null the texture to free (miniscule amounts of) memory
            }
            else
            {
                RenderingManager.AddToPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
                guiOpen = true;

                ModuleLight ml = getLightModule();

                Color32 c = getColour32RGBUnity(ml.lightR, ml.lightG, ml.lightB);

                rByte = Convert.ToInt16(c.r).ToString();
                gByte = Convert.ToInt16(c.g).ToString();
                bByte = Convert.ToInt16(c.b).ToString();
                hexColour = String.Format("#{0}{1}{2}", c.r.ToString("X"), c.g.ToString("X"), c.b.ToString("X"));
                updatePreviewFromHexColour(hexColour);

            }

        }

        public Color getColourRGBUnity(float r, float g, float b)
        {
            return new Color(r, g, b);
        }

        public Color32 getColour32RGBUnity(float r, float g, float b)
        {
            return (Color32)new Color(r, g, b);
        }

        public Color getColourRGB(byte r, byte b, byte g)
        {
            return (Color)new Color32(r, b, g, 255);
        }


        public Color getColourFromHex(string hex)
        {
            int bigint = Convert.ToInt32(hex.Replace("#", ""), 16);
            byte r = (byte)((bigint >> 16) & 255);
            byte g = (byte)((bigint >> 8) & 255);
            byte b = (byte)(bigint & 255);

            return getColourRGB(r, g, b);

        }

        public ModuleLight getLightModule()
        {
            ModuleLight ml = (ModuleLight)this.part.Modules.OfType<ModuleLight>().First();
            if (ml == null) {
                Destroy(this); // Destroy this module so that we don't constantly throw hundreds of IOEs.
                throw new InvalidOperationException("This part does not have a ModuleLight module!");
            }
            return ml;
        }

        public void testColourSetting()
        {
            ModuleLight ml = getLightModule();
            Color c = getColourRGBUnity(0.4f, 0.7f, 0.2f);
            float intensity = 0.5f;
            ml.lightR = c.r * intensity;
            ml.lightG = c.g * intensity;
            ml.lightB = c.b * intensity;
        }

        public Color getCheckedColourFromRGB(byte r, byte g, byte b)
        {

            // None of these are technically possible, but let's check for sanity's sake.

            if (r > 0xFF)
                r = 0xFF;
            else if (r < 0x0)
                r = 0x0;

            if (g > 0xFF)
                g = 0xFF;
            else if (g < 0x0)
                g = 0x0;

            if (b > 0xFF)
                b = 0xFF;
            else if (b < 0x0)
                b = 0x0;

            return getColourRGB(r, g, b);

        }

        public void updatePreviewFromHexColour(string hex)
        {
            Color c = getColourFromHex(hex);
            createPreviewTexture(c);
        }

        public void updatePreviewFromRGBColour(string r, string g, string b)
        {
            byte red, green, blue;
            red = Convert.ToByte(r);
            green = Convert.ToByte(g);
            blue = Convert.ToByte(b);
            Color c = getCheckedColourFromRGB(red, green, blue);
            createPreviewTexture(c);
        }

        public void createPreviewTexture(Color c)
        {
            Texture2D tex = new Texture2D(100, 50);
            for (int w = 0; w < 100; w++)
            {
                for (int h = 0; h < 50; h++)
                {
                    tex.SetPixel(w, h, c);
                }
            }
            tex.Apply();
            this.previewTexture = (Texture)tex;
        }

        public void OnDraw()
        {
            if (!hasSetupStyles)
            {
                GUISkin skin = HighLogic.Skin;

                hasSetupStyles = true;
                _winStyle = new GUIStyle(skin.window);
                _winStyle.fixedWidth = 300f;
                _winStyle.stretchHeight = _winStyle.stretchWidth = true;

                _labelStyle = new GUIStyle(skin.label);
                _labelStyle.stretchWidth = true;

                _labelStyleRichText = new GUIStyle(skin.label);
                _labelStyleRichText.stretchWidth = true;
                _labelStyleRichText.richText = true;

                _toggleStyle = new GUIStyle(skin.toggle);

                _buttonStyle = new GUIStyle(skin.button);

                _textFieldStyle = new GUIStyle(skin.textField);
                _textFieldStyle.wordWrap = false;


            }
            _winPos = GUILayout.Window(+this.part.GetInstanceID(), _winPos, OnWindow, "Advanced Colour Options", _winStyle, GUILayout.MinHeight(300f));
        }

        public void OnWindow(int id)
        {
            //Debug.Log("ONWINDOW");
            GUILayout.BeginVertical();
            //Debug.Log("After Vert");
            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    Event.current.Use();
                    hexColour = hexColour.Replace(Environment.NewLine, "");
                    rByte = rByte.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r","");
                    gByte = gByte.Replace(Environment.NewLine, "");
                    bByte = bByte.Replace(Environment.NewLine, "");
                    if (hexInput)
                    {
                        /*if (!hexColour.StartsWith("#"))
                            hexColour = "#" + hexColour;*/
                        updatePreviewFromHexColour(hexColour);
                        Color c1 = getColourFromHex(hexColour);
                        Color32 c = getColourRGBUnity(c1.r, c1.g, c1.b);
                        rByte = Convert.ToInt16(c.r).ToString();
                        gByte = Convert.ToInt16(c.g).ToString();
                        bByte = Convert.ToInt16(c.b).ToString();
                    }
                    else
                    {
                        updatePreviewFromRGBColour(rByte, gByte, bByte);
                        Color c1 = getCheckedColourFromRGB(Convert.ToByte(rByte), Convert.ToByte(gByte), Convert.ToByte(bByte));
                        Color32 c = getColour32RGBUnity(c1.r, c1.g, c1.b);
                        hexColour = String.Format("#{0}{1}{2}", c.r.ToString("X"), c.g.ToString("X"), c.b.ToString("X"));
                    }
                }
            }
            //Debug.Log("After Enter test");

            hexInput = GUILayout.Toggle(hexInput, "Use Hex input", _toggleStyle, GUILayout.ExpandWidth(true));
            //Debug.Log("After hex check");

            if (!hexInput)
            {

                GUILayout.BeginHorizontal(GUILayout.MinWidth(200f), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));


                GUILayout.Label("R:", _labelStyle);
                rByte = GUILayout.TextField(rByte, 3, _textFieldStyle, GUILayout.MinWidth(40f)).Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
                GUILayout.Label("G:", _labelStyle);
                gByte = GUILayout.TextField(gByte, 3, _textFieldStyle, GUILayout.MinWidth(40f)).Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
                GUILayout.Label("B:", _labelStyle);
                bByte = GUILayout.TextField(bByte, 3, _textFieldStyle, GUILayout.MinWidth(40f)).Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");


                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();


                GUILayout.Label("Hex input:");
                hexColour = GUILayout.TextField(hexColour.Replace("#", ""), 6, _textFieldStyle, GUILayout.MinWidth(60f)).Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");


                GUILayout.EndHorizontal();
            }
            GUILayout.Label("Press ENTER to confirm your entry!", _labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            GUILayout.Label(previewTexture, _labelStyle, GUILayout.MinWidth(100f), GUILayout.MinHeight(50f), GUILayout.ExpandWidth(true));
            GUILayout.Label("NOTE: Preview may not match actual light colour exactly!", _labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            applyToSymmetry = GUILayout.Toggle(applyToSymmetry, "Apply to all symmetry counterparts", _toggleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);
            GUILayout.Label("Please note that because KSP uses floats for its colours, conversion may not be 100% accurate. <color=red>#BADA55</color> may become <color=red>#BADA54</color> for exmaple.", _labelStyleRichText);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply", _buttonStyle, GUILayout.ExpandWidth(true)))
            {
                applyColourToLight(applyToSymmetry);
            }

            if (GUILayout.Button("Accept", _buttonStyle, GUILayout.ExpandWidth(true)))
            {
                applyColourToLight(applyToSymmetry);
                showGUI();
            }

            if (GUILayout.Button("Close", _buttonStyle, GUILayout.ExpandWidth(true)))
            {
                showGUI();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();

        }

        public void applyColourToLight(bool symmetry)
        {

            Color c = ((Texture2D)previewTexture).GetPixel(0, 0);

            if (symmetry)
            {
                foreach (Part p in this.part.symmetryCounterparts)
                {
                    ModuleLight ml = (ModuleLight)p.Modules["ModuleLight"];
                    if (ml == null)
                        continue;
                    ml.lightR = c.r;
                    ml.lightG = c.g;
                    ml.lightB = c.b;
                }
            }
            ModuleLight cml = (ModuleLight)this.part.Modules["ModuleLight"];
            if (cml == null) { return; }
            cml.lightR = c.r;
            cml.lightG = c.g;
            cml.lightB = c.b;
        }

    }
}
