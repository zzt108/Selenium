# Visual Smoke test for websites

This is a Proof of Concept. The goal is to take baseline snapshots of pages of a web site and later compare these snapshots to the actual look of the pages.

This is a fast and easy to maintain test, however analysing the test results need human decision sometime.

#Usage
The controm methods are implemented as unit tests in Selenium\NUnit.Tests\VisualCompareTest.cs

// This method creates the baseline snapshots
CreateBaselineImages
// If an error occurs this method can continue to create non existing baseline snapshots 
CreateBaselineImagesContinue
// This method creates the current state snapshots
CreateCurrentImages
// If an error occurs this method can continue to create non existing snapshots 
CreateCurrentImagesContinue
// Compares baseline and current snapshots and writes an HTML report with links to snapshot images
CompareResults