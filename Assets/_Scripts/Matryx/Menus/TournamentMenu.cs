﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Matryx;

public class TournamentMenu : MonoBehaviour
{
    private CalcManager calcManager;
    private MultiSelectFlexPanel submissionsPanel;

    [SerializeField]
    private SubmissionMenu submissionMenu;

    [SerializeField]
    private TMPro.TextMeshPro titleText;
    [SerializeField]
    private TMPro.TextMeshPro bountyText;
    [SerializeField]
    private TMPro.TextMeshPro descriptionText;

    [SerializeField]
    private GameObject contributeSection;

    [SerializeField]
    private GameObject winningSubmissions;
    [SerializeField]
    private GameObject loadingWinningSubmissions;

    static MatryxTournament tournament;
    public static MatryxTournament Tournament
    {
        get
        {
            return tournament;
        }
    }

    private Dictionary<string, MatryxSubmission> submissions = new Dictionary<string, MatryxSubmission>();

    internal class KeyboardInputResponder : FlexMenu.FlexMenuResponder
    {
        TournamentMenu submissionMenu;
        internal KeyboardInputResponder(TournamentMenu submissionMenu)
        {
            this.submissionMenu = submissionMenu;
        }

        public void Flex_ActionStart(string name, FlexActionableComponent sender, GameObject collider)
        {
            submissionMenu.HandleInput(sender.gameObject);
        }

        public void Flex_ActionEnd(string name, FlexActionableComponent sender, GameObject collider) { }
    }

    private Scroll scroll;
    const int maxTextLength = 400;

    JoyStickAggregator joyStickAggregator;
    FlexMenu flexMenu;

    public void Initialize(CalcManager calcManager)
    {
        this.calcManager = calcManager;

        scroll = GetComponentInChildren<Scroll>(true);
        flexMenu = GetComponent<FlexMenu>();
        KeyboardInputResponder responder = new KeyboardInputResponder(this);
        flexMenu.RegisterResponder(responder);
        submissionsPanel = GetComponentInChildren<MultiSelectFlexPanel>().Initialize();
        joyStickAggregator = scroll.GetComponent<JoyStickAggregator>();
    }

    public void HandleInput(GameObject source)
    {
        if (source.name == "Load_Button")
        {
            LoadMoreSubmissions();
        }
        else
        {
            MatryxSubmission submission = source.GetComponent<SubmissionContainer>().GetSubmission();
            DisplaySubmissionUI(submission);
        }
    }

    int page = 0;
    public void SetTournament(MatryxTournament newTournament)
    {
        if (tournament == null ||
            tournament.address != newTournament.address)
        {
            tournament = newTournament;
            UpdateHeaderUI();

            ClearSubmissions();
            var roundNumber = 0;
            loadingWinningSubmissions.SetActive(true);
            MatryxExplorer.RunFetchTournament(tournament.address, roundNumber, page, ProcessTournament);
        }
    }

    /// <summary>
    /// Loads the next page of submissions under a tournament
    /// </summary>
    public void LoadMoreSubmissions()
    {
        removeLoadButton();
        var roundNumber = 0;
        MatryxExplorer.RunFetchTournament(tournament.address, roundNumber, ++page, ProcessTournament);
    }

    /// <summary>
    /// Clears the list of submissions.
    /// </summary>
    public void ClearSubmissions()
    {
        page = 0;
        submissions.Clear();
        scroll.clear();
        winningSubmissions.SetActive(false);
    }

    public void ProcessTournament(object result)
    {
        loadingWinningSubmissions.SetActive(false);
        DisplaySubmissions((List<MatryxSubmission>)result);
    }

    /*
    public void ProcessTournamentOLD(string jsonString)
    {
        JSONObject jsonObject = new JSONObject(jsonString);
        jsonObject.GetField("results", delegate (JSONObject results)
        {
            results.GetField(tournament.address, delegate (JSONObject jsonTournament)
            {
                var title = jsonTournament.GetField("title").str;
                var bounty = jsonTournament.GetField("bounty").f;
                var description = jsonTournament.GetField("description").str;

                tournament.title = title;
                tournament.bounty = new long?((long)bounty);
                tournament.description = description;
                UpdateHeaderUI();

                List<JSONObject> submissionsList = null;
                List<Matryx_Submission> newSubmissions = new List<Matryx_Submission>();
                jsonTournament.GetField("submissions", delegate (JSONObject jsonSubmissions)
                {
                    submissionsList = jsonSubmissions.list;
                    foreach (JSONObject jsonSubmission in submissionsList)
                    {
                        string submissionAddress = jsonSubmission.GetField("address").str;
                        string submissionTitle = jsonSubmission.GetField("title").str;

                        Matryx_Submission aSubmission = new Matryx_Submission(submissionTitle, submissionAddress);
                        submissions.Add(submissionAddress, aSubmission);
                        newSubmissions.Add(aSubmission);
                    }

                    DisplaySubmissions(newSubmissions);
                });
            });
        });
    }
    */

    private void UpdateHeaderUI()
    {
        titleText.text = tournament.title;
        bountyText.text = "Reward: " + tournament.bounty + " MTX";
        descriptionText.text = tournament.getDescription();
        contributeSection.SetActive(tournament.currentRoundState.Equals("isOpen"));
    }

    public void DisplaySubmissionUI(MatryxSubmission submission)
    {
        submissionMenu.SetSubmission(submission);
        submissionMenu.gameObject.GetComponent<AnimationHandler>().OpenMenu();
    }

    GameObject loadButton;
    public void DisplaySubmissions(List<MatryxSubmission> submissions)
    {
        if(submissions.Count == 0)
        {
            winningSubmissions.SetActive(false);
            return;
        }

        winningSubmissions.SetActive(true);
        foreach (MatryxSubmission submission in submissions)
        {
            GameObject button = createButton(submission);
            button.SetActive(false);
            submissionsPanel.AddAction(button.GetComponent<FlexButtonComponent>());
        }

        // TODO: Decide: Keep this?
        //loadButton = createLoadButton();
        //submissionsPanel.AddAction(loadButton.GetComponent<FlexButtonComponent>());
    }

    private GameObject createButton(MatryxSubmission submission)
    {
        GameObject button = Instantiate(Resources.Load("Submission_Cell", typeof(GameObject))) as GameObject;
        button.transform.SetParent(submissionsPanel.transform);
        button.transform.localScale = Vector3.one;

        button.name = submission.details.title;
        button.GetComponent<SubmissionContainer>().SetSubmission(submission);

        button.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = submission.details.title;

        scroll.addObject(button.transform);
        joyStickAggregator.AddForwarder(button.GetComponentInChildren<JoyStickForwarder>());

        return button;
    }

    private GameObject createReloadButton()
    {
        GameObject button = Instantiate(Resources.Load("Submission_Cell", typeof(GameObject))) as GameObject;
        button.transform.SetParent(submissionsPanel.transform);
        button.transform.localScale = Vector3.one;

        button.name = "Load_Button";

        button.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = "(Reload Tournaments)";

        scroll.addObject(button.transform);
        joyStickAggregator.AddForwarder(button.GetComponentInChildren<JoyStickForwarder>());

        return button;
    }

    private void removeLoadButton()
    {
        if (loadButton != null)
        {
            List<Transform> loadButtonTransform = new List<Transform>();
            loadButtonTransform.Add(loadButton.transform);
            scroll.deleteObjects(new List<Transform>(loadButtonTransform));

            Destroy(loadButton);
        }
    }
}
