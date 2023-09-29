# import the necessary packages
import numpy as np
import imutils
import cv2


def align_images(image, template, imethod, maxFeatures=500, keepPercent=0.15,
                 debug=False):
    if imethod == 'orb':
        # convert both the input image and template to grayscale
        imageGray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        imageGray = cv2.threshold(imageGray, 200, 255, cv2.THRESH_BINARY)[1]

        templateGray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

        # use ORB to detect keypoints and extract (binary) local
        # invariant features
        orb = cv2.ORB_create(maxFeatures)
        (kpsA, descsA) = orb.detectAndCompute(imageGray, None)
        (kpsB, descsB) = orb.detectAndCompute(templateGray, None)

        # match the features
        method = cv2.DESCRIPTOR_MATCHER_BRUTEFORCE_HAMMING
        matcher = cv2.DescriptorMatcher_create(method)
        matches = matcher.match(descsA, descsB, None)

        # sort the matches by their distance (the smaller the distance,
        # the "more similar" the features are)
        matches = sorted(matches, key=lambda x: x.distance, reverse=False)

        # keep only the top matches
        keep = int(len(matches) * keepPercent)
        matches = matches[:keep]

        # check to see if we should visualize the matched keypoints
        if debug:
            matchedVis = cv2.drawMatches(image, kpsA, template, kpsB,
                                        matches, None)
            matchedVis = imutils.resize(matchedVis, width=1000)
            cv2.imshow("Matched Keypoints", matchedVis)
            cv2.waitKey(0)

        # allocate memory for the keypoints (x, y)-coordinates from the
        # top matches -- we'll use these coordinates to compute our
        # homography matrix
        ptsA = np.zeros((len(matches), 2), dtype="float")
        ptsB = np.zeros((len(matches), 2), dtype="float")

        # loop over the top matches
        for (i, m) in enumerate(matches):
            # indicate that the two keypoints in the respective images
            # map to each other
            ptsA[i] = kpsA[m.queryIdx].pt
            ptsB[i] = kpsB[m.trainIdx].pt

        # compute the homography matrix between the two sets of matched
        # points
        (H, mask) = cv2.findHomography(ptsA, ptsB, method=cv2.RANSAC)

        # use the homography matrix to align the images
        (h, w) = template.shape[:2]
        aligned = cv2.warpPerspective(image, H, (w, h))

        # return the aligned image
        return aligned
    
    elif imethod == 'sift':
        # convert both the input image and template to grayscale
        imageGray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        imageGray = cv2.threshold(imageGray, 200, 255, cv2.THRESH_BINARY)[1]

        templateGray = cv2.cvtColor(template, cv2.COLOR_BGR2GRAY)

        #sift
        sift = cv2.xfeatures2d.SIFT_create()
        (kpsA, descsA) = sift.detectAndCompute(imageGray,None)
        (kpsB, descsB) = sift.detectAndCompute(templateGray,None)

        # match the features
        bf = cv2.BFMatcher(cv2.NORM_L1, crossCheck=True)
        matches = bf.match(descsA,descsB)
        matches = sorted(matches, key = lambda x:x.distance, reverse=False)

        # keep only the top matches
        keep = int(len(matches) * keepPercent)
        matches = matches[:keep]

        # check to see if we should visualize the matched keypoints
        if debug:
            matchedVis = cv2.drawMatches(image, kpsA, template, kpsB,
                                        matches, None)
            matchedVis = imutils.resize(matchedVis, width=1000)
            cv2.imshow("Matched Keypoints", matchedVis)
            cv2.waitKey(0)

        # allocate memory for the keypoints (x, y)-coordinates from the
        # top matches -- we'll use these coordinates to compute our
        # homography matrix
        ptsA = np.zeros((len(matches), 2), dtype="float")
        ptsB = np.zeros((len(matches), 2), dtype="float")

        # loop over the top matches
        for (i, m) in enumerate(matches):
            # indicate that the two keypoints in the respective images
            # map to each other
            ptsA[i] = kpsA[m.queryIdx].pt
            ptsB[i] = kpsB[m.trainIdx].pt

        # compute the homography matrix between the two sets of matched
        # points
        (H, mask) = cv2.findHomography(ptsA, ptsB, method=cv2.RANSAC)

        # use the homography matrix to align the images
        (h, w) = template.shape[:2]
        aligned = cv2.warpPerspective(image, H, (w, h))

        # return the aligned image
        return aligned
    
    elif imethod == 'combined':
        return align_images_combined(image, template)
    
def align_images_combined(image, template, maxFeatures=500, keepPercent=0.2, debug=False):

    # Detect and extract features from the image and template using both ORB and SIFT.
    orb = cv2.ORB_create(maxFeatures)
    sift = cv2.xfeatures2d.SIFT_create()

    (kpsA_orb, descsA_orb) = orb.detectAndCompute(image, None)
    (kpsB_orb, descsB_orb) = orb.detectAndCompute(template, None)

    (kpsA_sift, descsA_sift) = sift.detectAndCompute(image, None)
    (kpsB_sift, descsB_sift) = sift.detectAndCompute(template, None)

    # Match the features from the two methods using a brute-force matcher.
    bf = cv2.BFMatcher(cv2.NORM_L1, crossCheck=True)

    matchesA_orb = bf.match(descsA_orb, descsB_orb)
    matchesA_sift = bf.match(descsA_sift, descsB_sift)

    # Sort the matches by their distance, and keep only the top matches.
    matchesA_orb = sorted(matchesA_orb, key = lambda x:x.distance)
    matchesA_sift = sorted(matchesA_sift, key = lambda x:x.distance)

    keep = int(len(matchesA_orb) * keepPercent)

    matchesA_orb = matchesA_orb[:keep]
    matchesA_sift = matchesA_sift[:keep]

    # Combine the matches from the two methods.
    matchesA = matchesA_orb + matchesA_sift

    # Compute the homography matrix between the two sets of matched points.
    kpsA = kpsA_sift + kpsA_orb
    kpsB = kpsB_sift + kpsB_orb
    

    # allocate memory for the keypoints (x, y)-coordinates from the
    # top matches -- we'll use these coordinates to compute our
    # homography matrix
    ptsA = np.zeros((len(matchesA), 2), dtype="float")
    ptsB = np.zeros((len(matchesA), 2), dtype="float")

    # loop over the top matches
    for (i, m) in enumerate(matchesA):
        # indicate that the two keypoints in the respective images
        # map to each other
        ptsA[i] = kpsA[m.queryIdx].pt
        ptsB[i] = kpsB[m.trainIdx].pt

    # Use the homography matrix to align the images.
    (H, mask) = cv2.findHomography(ptsA, ptsB, method=cv2.RANSAC)
    (h, w) = template.shape[:2]
    aligned = cv2.warpPerspective(image, H, (w, h))

    # Return the aligned image.
    return aligned

