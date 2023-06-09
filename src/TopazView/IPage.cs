﻿using Tenray.Topaz.API;

namespace Tenray.TopazView;

/// <summary>
/// The public interface of the page object that is exposed to the view script.
/// The naming convention for public methods and properties follows common JavaScript conventions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is a sealed class exposed to JSEngine.")]
public interface IPage
{
    /// <summary>
    /// If the layout set in the page on server side, 
    /// this will be used instead of the layout defined in the view.
    /// </summary>
    string layout { get; set; }

    /// <summary>
    /// The data object can be used to transfer data between views, sections and scripts.
    /// </summary>
    JsObject data { get; set; }

    /// <summary>
    /// Skin value to be used to define skin.
    /// </summary>
    string skin { get; }

    /// <summary>
    /// Appends raw string to the page.
    /// This method is unsafe for user inputs.
    /// You must sanitize user input using server side validation
    /// in order to prevent XSS attacks.
    /// Sanitizing the value in render stage,
    /// makes rendering slower which is not good!
    /// Make sure you sanitize all user inputs
    /// that is printed with page.Raw()
    /// </summary>
    /// <param name="value">The raw string that is being 
    /// appended to the page.</param>
    void raw(object value);

    /// <summary>
    /// Appends HTML Encoded string to the page.
    /// </summary>
    /// <param name="value"></param>
    void write(object value);

    /// <summary>
    /// Renders a view indicated by a relative or absolute path.
    /// </summary>
    /// <param name="view"></param>
    void renderView(object view);

    /// <summary>
    /// Renders body if this view was called as a layout of another view.
    /// </summary>
    void renderBody();

    /// <summary>
    /// Renders section in respect to the following order:
    /// 1. Current view
    /// 2. In rendering body
    /// 3. In layout.
    /// </summary>
    void renderSection(object section);

    /// <summary>
    /// Renders a section of a view indicated by a relative or absolute path.
    /// </summary>
    /// <param name="view"></param>
    /// <param name="section"></param>
    void renderViewSection(object view, object section);

    /// <summary>
    /// Runs script section in respect to the following order:
    /// 1. Current view
    /// 2. In rendering body
    /// 3. In layout.
    /// </summary>
    void runScript(object script);

    /// <summary>
    /// Runs a script section of a view indicated by a relative or absolute path.
    /// </summary>
    /// <param name="view"></param>
    /// <param name="script"></param>
    void runViewScript(object view, object script);

    void renderScopeSection(object section);

    void renderBodySection(object section);

    void renderLayoutSection(object section);

    void runScopeScript(object script);

    void runBodyScript(object script);

    void runLayoutScript(object script);

    /// <summary>
    /// Adds script to the list. This does not append anything.
    /// Allows adding script dependency anywhere in your view.
    /// Duplicated adds is also possible.
    /// Use WriteScripts to append all scripts to the page.
    /// (eg: used in your layout footer).
    /// </summary>
    /// <param name="script">The URL of the script</param>
    void addScript(object script);

    /// <summary>
    /// Adds script with module flag.
    /// </summary>
    /// <param name="script">The URL of the script</param>
    void addModule(object script);

    /// <summary>
    /// Adds style to the list. This does not append anything.
    /// Allows adding style dependency anywhere in your view.
    /// Duplicated adds is also possible.
    /// Use WriteStyles to append all styles to the page.
    /// (eg: used in your layout footer).
    /// </summary>
    /// <param name="script">The URL of the script</param>
    void addStyle(object style);

    /// <summary>
    /// Appends all script references in to the page.
    /// </summary>
    void writeScripts();

    /// <summary>
    /// Appends all style references in to the page.
    /// </summary>
    void writeStyles();
}
